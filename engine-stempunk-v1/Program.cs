using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StempunkRPG
{
    public enum TipoEquipamento
    {
        Ferramenta = 1,
        Invencao = 2
    }

    public sealed class Pericia
    {
        public string Nome { get; set; } = "";
        public int Valor { get; set; }

        public override string ToString() => $"{Nome} ({Valor})";
    }

    public sealed class Grupo
    {
        public string Nome { get; set; } = "";
        public int Costume { get; set; } // válido: -3, -2, 1, 2, 3

        public Dictionary<string, int> Status { get; set; } = new();
        public List<Pericia> Pericias { get; set; } = new();

        public void ExibirResumo()
        {
            Console.WriteLine($"[{Nome}] Costume: {Costume}");

            foreach (var status in Status)
                Console.WriteLine($"  Status - {status.Key}: {status.Value}");

            foreach (var pericia in Pericias)
                Console.WriteLine($"  Perícia - {pericia}");
        }
    }

    public sealed class Equipamento
    {
        public string Nome { get; set; } = "";
        public TipoEquipamento Tipo { get; set; }
        public int ModificadorDado { get; set; }
        public int ValorPotencial { get; set; }

        public override string ToString()
        {
            return $"{Nome} | {Tipo} | Mod. Dado: {ModificadorDado:+#;-#;0} | Potencial: {ValorPotencial}";
        }
    }

    public sealed class Personagem
    {
        public string Nome { get; set; } = "";

        public int Fisico { get; set; }
        public int Conhecimento { get; set; }
        public int Personalidade { get; set; }
        public int Desperto { get; set; }

        public int Total => Fisico + Conhecimento + Personalidade + Desperto;

        public Grupo Atletismo { get; set; } = new() { Nome = "Atletismo" };
        public Grupo ConhecimentoGrupo { get; set; } = new() { Nome = "Conhecimento" };
        public Grupo PersonalidadeGrupo { get; set; } = new() { Nome = "Personalidade" };
        public Grupo DespertoGrupo { get; set; } = new() { Nome = "Desperto" };

        public List<Equipamento> Equipamentos { get; set; } = new();

        public Grupo ObterGrupo(string nomeGrupo)
        {
            return nomeGrupo switch
            {
                "Atletismo" => Atletismo,
                "Conhecimento" => ConhecimentoGrupo,
                "Personalidade" => PersonalidadeGrupo,
                "Desperto" => DespertoGrupo,
                _ => throw new InvalidOperationException("Grupo inválido.")
            };
        }

        public int ObterPotencialBase(string nomeGrupo)
        {
            return nomeGrupo switch
            {
                "Atletismo" => Fisico,
                "Conhecimento" => Conhecimento,
                "Personalidade" => Personalidade,
                "Desperto" => Desperto,
                _ => 0
            };
        }

        public override string ToString()
        {
            return $"{Nome} | Físico:{Fisico} Conhecimento:{Conhecimento} Personalidade:{Personalidade} Desperto:{Desperto} Total:{Total}";
        }
    }

    public sealed class BancoDeDados
    {
        public List<Personagem> Personagens { get; set; } = new();
    }

    public static class Persistencia
    {
        private static readonly string CaminhoArquivo = "dados.json";

        public static BancoDeDados Carregar()
        {
            try
            {
                if (!File.Exists(CaminhoArquivo))
                    return new BancoDeDados();

                string json = File.ReadAllText(CaminhoArquivo);

                if (string.IsNullOrWhiteSpace(json))
                    return new BancoDeDados();

                return JsonSerializer.Deserialize<BancoDeDados>(json) ?? new BancoDeDados();
            }
            catch
            {
                Console.WriteLine("Falha ao carregar dados.json. Um banco vazio será iniciado.");
                return new BancoDeDados();
            }
        }

        public static void Salvar(BancoDeDados banco)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(banco, options);
            File.WriteAllText(CaminhoArquivo, json);
        }
    }

    public sealed class ResultadoTeste
    {
        public string NomePersonagem { get; set; } = "";
        public string Grupo { get; set; } = "";
        public string StatusNome { get; set; } = "";
        public int ValorStatus { get; set; }
        public string PericiaNome { get; set; } = "Nenhuma";
        public int ValorPericia { get; set; }
        public string EquipamentoNome { get; set; } = "Nenhum";
        public TipoEquipamento? TipoEquipamento { get; set; }
        public int ModificadorDadoEquipamento { get; set; }
        public int PotencialEquipamento { get; set; }
        public int Costume { get; set; }
        public int DadoNatural { get; set; }
        public int TotalNoDado { get; set; }
        public double Percentual { get; set; }
        public int PotencialBaseGrupo { get; set; }
        public int PotencialUsado { get; set; }
        public double ResultadoPotencial { get; set; }
        public int PotencialNecessario { get; set; }
        public double DiferencaParaNecessario { get; set; }
        public string Classificacao { get; set; } = "";
    }

    public static class Regras
    {
        public static string DescreverRolagemCostume(int costume)
        {
            if (costume > 0)
                return $"Role {costume}d20 e pegue o melhor.";
            return $"Role {Math.Abs(costume)}d20 e pegue o pior.";
        }

        public static int CalcularTotalNoDado(int dadoNatural, int status, int pericia, int modificadorEquipamento)
        {
            return dadoNatural + status + pericia + modificadorEquipamento;
        }

        public static double ConverterPercentual(int totalNoDado)
        {
            if (totalNoDado < 1)
                totalNoDado = 1;

            return totalNoDado * 0.05;
        }

        public static int CalcularPotencialUsado(int potencialBaseGrupo, Equipamento? equipamento)
        {
            if (equipamento == null)
                return potencialBaseGrupo;

            if (equipamento.Tipo == TipoEquipamento.Ferramenta)
                return potencialBaseGrupo + equipamento.ValorPotencial;

            return equipamento.ValorPotencial;
        }

        public static double CalcularResultadoPotencial(int potencialUsado, int totalNoDado)
        {
            double percentual = ConverterPercentual(totalNoDado);
            return potencialUsado * percentual;
        }

        public static string ClassificarResultado(double resultado, int necessario)
        {
            double diferenca = resultado - necessario;

            if (diferenca <= -10)
                return "Falha crítica";
            if (diferenca < 0)
                return "Falha relativa";
            if (diferenca < 10)
                return "Passou";
            if (diferenca < 20)
                return "Passou muito bem";

            return "Passou tranquilamente";
        }
    }

    internal static class Program
    {
        private static BancoDeDados banco = new();

        private static void Main()
        {
            banco = Persistencia.Carregar();

            bool executando = true;

            while (executando)
            {
                Console.Clear();
                Console.WriteLine("=== STEMPUNK RPG ===");
                Console.WriteLine("1 - Cadastrar personagem");
                Console.WriteLine("2 - Cadastrar equipamento");
                Console.WriteLine("3 - Listar personagens");
                Console.WriteLine("4 - Executar teste");
                Console.WriteLine("5 - Executar confronto");
                Console.WriteLine("6 - Excluir personagem");
                Console.WriteLine("7 - Excluir equipamento");
                Console.WriteLine("8 - Editar personagem");
                Console.WriteLine("9 - Editar equipamento");
                Console.WriteLine("0 - Sair");
                Console.Write("Escolha: ");

                string? opcao = Console.ReadLine();

                try
                {
                    switch (opcao)
                    {
                        case "1":
                            CadastrarPersonagem();
                            break;
                        case "2":
                            CadastrarEquipamento();
                            break;
                        case "3":
                            ListarPersonagens();
                            break;
                        case "4":
                            ExecutarTeste();
                            break;
                        case "5":
                            ExecutarConfronto();
                            break;
                        case "6":
                            ExcluirPersonagem();
                            break;
                        case "7":
                            ExcluirEquipamento();
                            break;
                        case "8":
                            EditarPersonagem();
                            break;
                        case "9":
                            EditarEquipamento();
                            break;
                        case "0":
                            Persistencia.Salvar(banco);
                            executando = false;
                            break;
                        default:
                            Console.WriteLine("Opção inválida.");
                            Pausar();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                    Pausar();
                }
            }
        }

        private static void CadastrarPersonagem()
        {
            Console.Clear();
            Console.WriteLine("=== CADASTRO DE PERSONAGEM ===");

            Personagem p = new();

            Console.Write("Nome do personagem: ");
            p.Nome = Console.ReadLine() ?? "";

            Console.WriteLine("\n=== POTENCIAIS GERAIS ===");
            p.Fisico = LerInt("Físico: ");
            p.Conhecimento = LerInt("Conhecimento: ");
            p.Personalidade = LerInt("Personalidade: ");
            p.Desperto = LerInt("Desperto: ");

            Console.WriteLine($"Total: {p.Total}");

            CadastrarGrupo(p.Atletismo, new[] { "Força", "Agilidade", "Resistencia", "Precisão" });
            CadastrarGrupo(p.ConhecimentoGrupo, new[] { "Intelecto", "Percepção", "Dedução", "Aplicar" });
            CadastrarGrupo(p.PersonalidadeGrupo, new[] { "Lábia", "Carisma", "Seduzir", "Resistencia Mental" });
            CadastrarGrupo(p.DespertoGrupo, new[] { "Sorte" });

            banco.Personagens.Add(p);
            Persistencia.Salvar(banco);

            Console.WriteLine("\nPersonagem cadastrado com sucesso.");
            Pausar();
        }

        private static void CadastrarGrupo(Grupo grupo, string[] nomesStatus)
        {
            Console.WriteLine($"\n=== {grupo.Nome} ===");
            grupo.Costume = LerCostume($"Costume de {grupo.Nome}: ");

            Console.WriteLine("Status:");
            foreach (string nome in nomesStatus)
            {
                grupo.Status[nome] = LerStatus($"{nome}: ");
            }

            Console.WriteLine("Perícias desse grupo. Deixe vazio para encerrar.");
            while (true)
            {
                Console.Write("Nome da perícia: ");
                string nomePericia = Console.ReadLine() ?? "";

                if (string.IsNullOrWhiteSpace(nomePericia))
                    break;

                int valor = LerPericia("Valor da perícia: ");

                grupo.Pericias.Add(new Pericia
                {
                    Nome = nomePericia,
                    Valor = valor
                });
            }
        }

        private static void CadastrarEquipamento()
        {
            Console.Clear();
            Console.WriteLine("=== CADASTRAR EQUIPAMENTO ===");

            Personagem? personagem = SelecionarPersonagem();
            if (personagem == null)
                return;

            Equipamento equipamento = new();

            Console.Write("Nome do equipamento: ");
            equipamento.Nome = Console.ReadLine() ?? "";

            Console.WriteLine("Tipo:");
            Console.WriteLine("1 - Ferramenta");
            Console.WriteLine("2 - Invenção");
            equipamento.Tipo = (TipoEquipamento)LerInt("Escolha: ");

            equipamento.ModificadorDado = LerInt("Modificador de dado do equipamento: ");
            equipamento.ValorPotencial = LerInt("Valor potencial do equipamento: ");

            personagem.Equipamentos.Add(equipamento);
            Persistencia.Salvar(banco);

            Console.WriteLine("Equipamento cadastrado com sucesso.");
            Pausar();
        }

        private static void ListarPersonagens()
        {
            Console.Clear();
            Console.WriteLine("=== PERSONAGENS CADASTRADOS ===");

            if (!banco.Personagens.Any())
            {
                Console.WriteLine("Nenhum personagem cadastrado.");
                Pausar();
                return;
            }

            foreach (Personagem p in banco.Personagens)
            {
                Console.WriteLine(p);
                Console.WriteLine();

                p.Atletismo.ExibirResumo();
                Console.WriteLine();
                p.ConhecimentoGrupo.ExibirResumo();
                Console.WriteLine();
                p.PersonalidadeGrupo.ExibirResumo();
                Console.WriteLine();
                p.DespertoGrupo.ExibirResumo();
                Console.WriteLine();

                if (p.Equipamentos.Any())
                {
                    Console.WriteLine("Equipamentos:");
                    foreach (Equipamento eq in p.Equipamentos)
                        Console.WriteLine($"  - {eq}");
                }
                else
                {
                    Console.WriteLine("Equipamentos: nenhum");
                }

                Console.WriteLine(new string('-', 50));
            }

            Pausar();
        }

        private static void ExecutarTeste()
        {
            Console.Clear();
            Console.WriteLine("=== TESTE ===");

            Personagem? personagem = SelecionarPersonagem();
            if (personagem == null)
                return;

            string nomeGrupo = SelecionarGrupo();
            Grupo grupo = personagem.ObterGrupo(nomeGrupo);

            Console.WriteLine();
            Console.WriteLine($"Costume selecionado: {grupo.Costume}");
            Console.WriteLine(Regras.DescreverRolagemCostume(grupo.Costume));

            Equipamento? equipamento = SelecionarEquipamentoOpcional(personagem);

            string statusNome = SelecionarStatus(grupo);
            int valorStatus = grupo.Status[statusNome];

            Pericia? pericia = SelecionarPericiaOpcional(grupo);
            int valorPericia = pericia?.Valor ?? 0;

            int dadoNatural = LerInt("Valor do dado tirado pelo jogador: ");

            int totalNoDado = Regras.CalcularTotalNoDado(
                dadoNatural,
                valorStatus,
                valorPericia,
                equipamento?.ModificadorDado ?? 0);

            int potencialBaseGrupo = personagem.ObterPotencialBase(nomeGrupo);
            int potencialUsado = Regras.CalcularPotencialUsado(potencialBaseGrupo, equipamento);
            double percentual = Regras.ConverterPercentual(totalNoDado);
            double resultadoPotencial = Regras.CalcularResultadoPotencial(potencialUsado, totalNoDado);

            int potencialNecessario = LerInt("Potencial necessário para o teste: ");
            double diferenca = resultadoPotencial - potencialNecessario;
            string classificacao = Regras.ClassificarResultado(resultadoPotencial, potencialNecessario);

            ResultadoTeste resultado = new()
            {
                NomePersonagem = personagem.Nome,
                Grupo = nomeGrupo,
                StatusNome = statusNome,
                ValorStatus = valorStatus,
                PericiaNome = pericia?.Nome ?? "Nenhuma",
                ValorPericia = valorPericia,
                EquipamentoNome = equipamento?.Nome ?? "Nenhum",
                TipoEquipamento = equipamento?.Tipo,
                ModificadorDadoEquipamento = equipamento?.ModificadorDado ?? 0,
                PotencialEquipamento = equipamento?.ValorPotencial ?? 0,
                Costume = grupo.Costume,
                DadoNatural = dadoNatural,
                TotalNoDado = totalNoDado,
                Percentual = percentual,
                PotencialBaseGrupo = potencialBaseGrupo,
                PotencialUsado = potencialUsado,
                ResultadoPotencial = resultadoPotencial,
                PotencialNecessario = potencialNecessario,
                DiferencaParaNecessario = diferenca,
                Classificacao = classificacao
            };

            ExibirResultadoTeste(resultado);
            Pausar();
        }

        private static void ExecutarConfronto()
        {
            Console.Clear();
            Console.WriteLine("=== CONFRONTO ===");

            Console.WriteLine("\n--- Primeiro personagem ---");
            Personagem? primeiro = SelecionarPersonagem();
            if (primeiro == null)
                return;

            ResultadoTeste r1 = ResolverConfrontoIndividual(primeiro);

            Console.WriteLine("\n--- Segundo personagem ---");
            Personagem? segundo = SelecionarPersonagem();
            if (segundo == null)
                return;

            ResultadoTeste r2 = ResolverConfrontoIndividual(segundo);

            Console.WriteLine("\n=== RESULTADO DO CONFRONTO ===");
            Console.WriteLine();
            ExibirResultadoTesteSemPausa(r1);
            Console.WriteLine();
            ExibirResultadoTesteSemPausa(r2);
            Console.WriteLine();

            double diferenca = r1.ResultadoPotencial - r2.ResultadoPotencial;

            if (diferenca > 0)
                Console.WriteLine($"{r1.NomePersonagem} ganhou com {diferenca:0.##} pontos.");
            else if (diferenca < 0)
                Console.WriteLine($"{r2.NomePersonagem} ganhou com {Math.Abs(diferenca):0.##} pontos.");
            else
                Console.WriteLine("Empate.");

            Pausar();
        }

        private static ResultadoTeste ResolverConfrontoIndividual(Personagem personagem)
        {
            Console.WriteLine($"\nPersonagem: {personagem.Nome}");

            string nomeGrupo = SelecionarGrupo();
            Grupo grupo = personagem.ObterGrupo(nomeGrupo);

            Console.WriteLine($"Costume selecionado: {grupo.Costume}");
            Console.WriteLine(Regras.DescreverRolagemCostume(grupo.Costume));

            Equipamento? equipamento = SelecionarEquipamentoOpcional(personagem);

            string statusNome = SelecionarStatus(grupo);
            int valorStatus = grupo.Status[statusNome];

            Pericia? pericia = SelecionarPericiaOpcional(grupo);
            int valorPericia = pericia?.Valor ?? 0;

            int dadoNatural = LerInt("Valor do dado tirado pelo jogador: ");

            int totalNoDado = Regras.CalcularTotalNoDado(
                dadoNatural,
                valorStatus,
                valorPericia,
                equipamento?.ModificadorDado ?? 0);

            int potencialBaseGrupo = personagem.ObterPotencialBase(nomeGrupo);
            int potencialUsado = Regras.CalcularPotencialUsado(potencialBaseGrupo, equipamento);
            double percentual = Regras.ConverterPercentual(totalNoDado);
            double resultadoPotencial = Regras.CalcularResultadoPotencial(potencialUsado, totalNoDado);

            return new ResultadoTeste
            {
                NomePersonagem = personagem.Nome,
                Grupo = nomeGrupo,
                StatusNome = statusNome,
                ValorStatus = valorStatus,
                PericiaNome = pericia?.Nome ?? "Nenhuma",
                ValorPericia = valorPericia,
                EquipamentoNome = equipamento?.Nome ?? "Nenhum",
                TipoEquipamento = equipamento?.Tipo,
                ModificadorDadoEquipamento = equipamento?.ModificadorDado ?? 0,
                PotencialEquipamento = equipamento?.ValorPotencial ?? 0,
                Costume = grupo.Costume,
                DadoNatural = dadoNatural,
                TotalNoDado = totalNoDado,
                Percentual = percentual,
                PotencialBaseGrupo = potencialBaseGrupo,
                PotencialUsado = potencialUsado,
                ResultadoPotencial = resultadoPotencial,
                PotencialNecessario = 0,
                DiferencaParaNecessario = 0,
                Classificacao = "Confronto"
            };
        }

        private static void EditarPersonagem()
        {
            Console.Clear();
            Console.WriteLine("=== EDITAR PERSONAGEM ===");

            Personagem? personagem = SelecionarPersonagem();
            if (personagem == null)
                return;

            bool editando = true;

            while (editando)
            {
                Console.Clear();
                Console.WriteLine($"=== EDITANDO {personagem.Nome} ===");
                Console.WriteLine($"Total atual calculado automaticamente: {personagem.Total}");
                Console.WriteLine("1 - Nome");
                Console.WriteLine("2 - Potenciais gerais");
                Console.WriteLine("3 - Grupo Atletismo");
                Console.WriteLine("4 - Grupo Conhecimento");
                Console.WriteLine("5 - Grupo Personalidade");
                Console.WriteLine("6 - Grupo Desperto");
                Console.WriteLine("0 - Voltar");
                Console.Write("Escolha: ");

                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1":
                        Console.Write("Novo nome: ");
                        personagem.Nome = Console.ReadLine() ?? personagem.Nome;
                        Persistencia.Salvar(banco);
                        break;
                    case "2":
                        personagem.Fisico = LerInt("Físico: ");
                        personagem.Conhecimento = LerInt("Conhecimento: ");
                        personagem.Personalidade = LerInt("Personalidade: ");
                        personagem.Desperto = LerInt("Desperto: ");
                        Console.WriteLine($"Novo total calculado automaticamente: {personagem.Total}");
                        Persistencia.Salvar(banco);
                        Pausar();
                        break;
                    case "3":
                        EditarGrupo(personagem.Atletismo);
                        Persistencia.Salvar(banco);
                        break;
                    case "4":
                        EditarGrupo(personagem.ConhecimentoGrupo);
                        Persistencia.Salvar(banco);
                        break;
                    case "5":
                        EditarGrupo(personagem.PersonalidadeGrupo);
                        Persistencia.Salvar(banco);
                        break;
                    case "6":
                        EditarGrupo(personagem.DespertoGrupo);
                        Persistencia.Salvar(banco);
                        break;
                    case "0":
                        editando = false;
                        break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        Pausar();
                        break;
                }
            }
        }

        private static void EditarGrupo(Grupo grupo)
        {
            bool editando = true;

            while (editando)
            {
                Console.Clear();
                Console.WriteLine($"=== EDITAR GRUPO {grupo.Nome} ===");
                Console.WriteLine($"Costume atual: {grupo.Costume}");
                Console.WriteLine("1 - Editar costume");
                Console.WriteLine("2 - Editar status");
                Console.WriteLine("3 - Editar perícias");
                Console.WriteLine("0 - Voltar");
                Console.Write("Escolha: ");

                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1":
                        grupo.Costume = LerCostume("Novo costume: ");
                        break;
                    case "2":
                        EditarStatusGrupo(grupo);
                        break;
                    case "3":
                        EditarPericiasGrupo(grupo);
                        break;
                    case "0":
                        editando = false;
                        break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        Pausar();
                        break;
                }
            }
        }

        private static void EditarStatusGrupo(Grupo grupo)
        {
            var lista = grupo.Status.Keys.ToList();
            if (!lista.Any())
            {
                Console.WriteLine("Esse grupo não possui status.");
                Pausar();
                return;
            }

            Console.WriteLine("Selecione o status para editar:");
            for (int i = 0; i < lista.Count; i++)
                Console.WriteLine($"{i + 1} - {lista[i]} ({grupo.Status[lista[i]]})");

            int escolha = LerInt("Escolha: ") - 1;
            if (escolha < 0 || escolha >= lista.Count)
            {
                Console.WriteLine("Status inválido.");
                Pausar();
                return;
            }

            string nome = lista[escolha];
            grupo.Status[nome] = LerStatus($"Novo valor para {nome}: ");
        }

        private static void EditarPericiasGrupo(Grupo grupo)
        {
            bool editando = true;

            while (editando)
            {
                Console.Clear();
                Console.WriteLine($"=== PERÍCIAS DE {grupo.Nome} ===");

                if (grupo.Pericias.Any())
                {
                    for (int i = 0; i < grupo.Pericias.Count; i++)
                        Console.WriteLine($"{i + 1} - {grupo.Pericias[i]}");
                }
                else
                {
                    Console.WriteLine("Nenhuma perícia cadastrada.");
                }

                Console.WriteLine("1 - Editar perícia");
                Console.WriteLine("2 - Adicionar perícia");
                Console.WriteLine("3 - Excluir perícia");
                Console.WriteLine("0 - Voltar");
                Console.Write("Escolha: ");

                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1":
                        if (!grupo.Pericias.Any())
                        {
                            Console.WriteLine("Nenhuma perícia para editar.");
                            Pausar();
                            break;
                        }
                        int idxEditar = LerInt("Número da perícia: ") - 1;
                        if (idxEditar < 0 || idxEditar >= grupo.Pericias.Count)
                        {
                            Console.WriteLine("Perícia inválida.");
                            Pausar();
                            break;
                        }
                        Console.Write("Novo nome: ");
                        grupo.Pericias[idxEditar].Nome = Console.ReadLine() ?? grupo.Pericias[idxEditar].Nome;
                        grupo.Pericias[idxEditar].Valor = LerPericia("Novo valor: ");
                        break;

                    case "2":
                        Console.Write("Nome da nova perícia: ");
                        string nomeNova = Console.ReadLine() ?? "";
                        if (!string.IsNullOrWhiteSpace(nomeNova))
                        {
                            grupo.Pericias.Add(new Pericia
                            {
                                Nome = nomeNova,
                                Valor = LerPericia("Valor da perícia: ")
                            });
                        }
                        break;

                    case "3":
                        if (!grupo.Pericias.Any())
                        {
                            Console.WriteLine("Nenhuma perícia para excluir.");
                            Pausar();
                            break;
                        }
                        int idxExcluir = LerInt("Número da perícia: ") - 1;
                        if (idxExcluir < 0 || idxExcluir >= grupo.Pericias.Count)
                        {
                            Console.WriteLine("Perícia inválida.");
                            Pausar();
                            break;
                        }
                        grupo.Pericias.RemoveAt(idxExcluir);
                        break;

                    case "0":
                        editando = false;
                        break;

                    default:
                        Console.WriteLine("Opção inválida.");
                        Pausar();
                        break;
                }
            }
        }

        private static void EditarEquipamento()
        {
            Console.Clear();
            Console.WriteLine("=== EDITAR EQUIPAMENTO ===");

            Personagem? personagem = SelecionarPersonagem();
            if (personagem == null)
                return;

            if (!personagem.Equipamentos.Any())
            {
                Console.WriteLine("Esse personagem não possui equipamentos cadastrados.");
                Pausar();
                return;
            }

            for (int i = 0; i < personagem.Equipamentos.Count; i++)
                Console.WriteLine($"{i + 1} - {personagem.Equipamentos[i]}");

            int escolha = LerInt("Escolha o equipamento: ") - 1;
            if (escolha < 0 || escolha >= personagem.Equipamentos.Count)
            {
                Console.WriteLine("Equipamento inválido.");
                Pausar();
                return;
            }

            var equipamento = personagem.Equipamentos[escolha];

            Console.Write("Novo nome: ");
            string novoNome = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(novoNome))
                equipamento.Nome = novoNome;

            Console.WriteLine("Tipo:");
            Console.WriteLine("1 - Ferramenta");
            Console.WriteLine("2 - Invenção");
            equipamento.Tipo = (TipoEquipamento)LerInt("Escolha: ");

            equipamento.ModificadorDado = LerInt("Novo modificador de dado: ");
            equipamento.ValorPotencial = LerInt("Novo valor potencial: ");

            Persistencia.Salvar(banco);
            Console.WriteLine("Equipamento editado com sucesso.");
            Pausar();
        }

        private static void ExcluirPersonagem()
        {
            Console.Clear();
            Console.WriteLine("=== EXCLUIR PERSONAGEM ===");

            if (!banco.Personagens.Any())
            {
                Console.WriteLine("Nenhum personagem cadastrado.");
                Pausar();
                return;
            }

            for (int i = 0; i < banco.Personagens.Count; i++)
            {
                Console.WriteLine($"{i + 1} - {banco.Personagens[i].Nome}");
            }

            int escolha = LerInt("Escolha o personagem para excluir: ") - 1;

            if (escolha < 0 || escolha >= banco.Personagens.Count)
            {
                Console.WriteLine("Personagem inválido.");
                Pausar();
                return;
            }

            string nome = banco.Personagens[escolha].Nome;

            Console.Write($"Tem certeza que deseja excluir {nome}? (s/n): ");
            string confirmacao = (Console.ReadLine() ?? "").Trim().ToLower();

            if (confirmacao == "s")
            {
                banco.Personagens.RemoveAt(escolha);
                Persistencia.Salvar(banco);
                Console.WriteLine("Personagem excluído com sucesso.");
            }
            else
            {
                Console.WriteLine("Exclusão cancelada.");
            }

            Pausar();
        }

        private static void ExcluirEquipamento()
        {
            Console.Clear();
            Console.WriteLine("=== EXCLUIR EQUIPAMENTO ===");

            Personagem? personagem = SelecionarPersonagem();
            if (personagem == null)
                return;

            if (!personagem.Equipamentos.Any())
            {
                Console.WriteLine("Esse personagem não possui equipamentos cadastrados.");
                Pausar();
                return;
            }

            for (int i = 0; i < personagem.Equipamentos.Count; i++)
            {
                Console.WriteLine($"{i + 1} - {personagem.Equipamentos[i]}");
            }

            int escolha = LerInt("Escolha o equipamento para excluir: ") - 1;

            if (escolha < 0 || escolha >= personagem.Equipamentos.Count)
            {
                Console.WriteLine("Equipamento inválido.");
                Pausar();
                return;
            }

            string nome = personagem.Equipamentos[escolha].Nome;

            Console.Write($"Tem certeza que deseja excluir {nome}? (s/n): ");
            string confirmacao = (Console.ReadLine() ?? "").Trim().ToLower();

            if (confirmacao == "s")
            {
                personagem.Equipamentos.RemoveAt(escolha);
                Persistencia.Salvar(banco);
                Console.WriteLine("Equipamento excluído com sucesso.");
            }
            else
            {
                Console.WriteLine("Exclusão cancelada.");
            }

            Pausar();
        }

        private static void ExibirResultadoTeste(ResultadoTeste r)
        {
            ExibirResultadoTesteSemPausa(r);
        }

        private static void ExibirResultadoTesteSemPausa(ResultadoTeste r)
        {
            Console.WriteLine("=== RESULTADO ===");
            Console.WriteLine($"Personagem: {r.NomePersonagem}");
            Console.WriteLine($"Grupo/Costume: {r.Grupo} ({r.Costume})");
            Console.WriteLine($"Status: {r.StatusNome} = {r.ValorStatus}");
            Console.WriteLine($"Perícia: {r.PericiaNome} = {r.ValorPericia}");
            Console.WriteLine($"Equipamento: {r.EquipamentoNome}");

            if (r.TipoEquipamento.HasValue)
            {
                Console.WriteLine($"Tipo de equipamento: {r.TipoEquipamento.Value}");
                Console.WriteLine($"Modificador de dado do equipamento: {r.ModificadorDadoEquipamento:+#;-#;0}");
                Console.WriteLine($"Valor potencial do equipamento: {r.PotencialEquipamento}");
            }

            Console.WriteLine($"Dado natural: {r.DadoNatural}");
            Console.WriteLine($"Total no dado: {r.TotalNoDado}");
            Console.WriteLine($"Conversão percentual: {r.Percentual * 100:0}%");
            Console.WriteLine($"Potencial base do grupo: {r.PotencialBaseGrupo}");
            Console.WriteLine($"Potencial usado: {r.PotencialUsado}");
            Console.WriteLine($"Resultado potencial final: {r.ResultadoPotencial:0.##}");

            if (r.Classificacao != "Confronto")
            {
                Console.WriteLine($"Potencial necessário: {r.PotencialNecessario}");
                Console.WriteLine($"Diferença para o necessário: {r.DiferencaParaNecessario:0.##}");
                Console.WriteLine($"Classificação: {r.Classificacao}");
            }
        }

        private static Personagem? SelecionarPersonagem()
        {
            if (!banco.Personagens.Any())
            {
                Console.WriteLine("Nenhum personagem cadastrado.");
                Pausar();
                return null;
            }

            Console.WriteLine("Selecione o personagem:");
            for (int i = 0; i < banco.Personagens.Count; i++)
                Console.WriteLine($"{i + 1} - {banco.Personagens[i].Nome}");

            int escolha = LerInt("Escolha: ") - 1;

            if (escolha < 0 || escolha >= banco.Personagens.Count)
                throw new InvalidOperationException("Personagem inválido.");

            return banco.Personagens[escolha];
        }

        private static string SelecionarGrupo()
        {
            Console.WriteLine("\nSelecione o costume/grupo:");
            Console.WriteLine("1 - Atletismo");
            Console.WriteLine("2 - Conhecimento");
            Console.WriteLine("3 - Personalidade");
            Console.WriteLine("4 - Desperto");

            int escolha = LerInt("Escolha: ");

            return escolha switch
            {
                1 => "Atletismo",
                2 => "Conhecimento",
                3 => "Personalidade",
                4 => "Desperto",
                _ => throw new InvalidOperationException("Grupo inválido.")
            };
        }

        private static Equipamento? SelecionarEquipamentoOpcional(Personagem personagem)
        {
            if (!personagem.Equipamentos.Any())
            {
                Console.WriteLine("Esse personagem não possui equipamentos cadastrados.");
                return null;
            }

            Console.WriteLine("\nSelecione o item/equipamento:");
            Console.WriteLine("0 - Nenhum");

            for (int i = 0; i < personagem.Equipamentos.Count; i++)
                Console.WriteLine($"{i + 1} - {personagem.Equipamentos[i]}");

            int escolha = LerInt("Escolha: ");

            if (escolha == 0)
                return null;

            int indice = escolha - 1;

            if (indice < 0 || indice >= personagem.Equipamentos.Count)
                throw new InvalidOperationException("Equipamento inválido.");

            return personagem.Equipamentos[indice];
        }

        private static string SelecionarStatus(Grupo grupo)
        {
            if (!grupo.Status.Any())
                throw new InvalidOperationException("Esse grupo não possui status cadastrados.");

            Console.WriteLine("\nSelecione o status:");
            var lista = grupo.Status.Keys.ToList();

            for (int i = 0; i < lista.Count; i++)
                Console.WriteLine($"{i + 1} - {lista[i]} ({grupo.Status[lista[i]]})");

            int escolha = LerInt("Escolha: ") - 1;

            if (escolha < 0 || escolha >= lista.Count)
                throw new InvalidOperationException("Status inválido.");

            return lista[escolha];
        }

        private static Pericia? SelecionarPericiaOpcional(Grupo grupo)
        {
            if (!grupo.Pericias.Any())
            {
                Console.WriteLine("Esse grupo não possui perícias cadastradas.");
                return null;
            }

            Console.WriteLine("\nSelecione a perícia:");
            Console.WriteLine("0 - Nenhuma");

            for (int i = 0; i < grupo.Pericias.Count; i++)
                Console.WriteLine($"{i + 1} - {grupo.Pericias[i]}");

            int escolha = LerInt("Escolha: ");

            if (escolha == 0)
                return null;

            int indice = escolha - 1;

            if (indice < 0 || indice >= grupo.Pericias.Count)
                throw new InvalidOperationException("Perícia inválida.");

            return grupo.Pericias[indice];
        }

        private static int LerInt(string mensagem)
        {
            while (true)
            {
                Console.Write(mensagem);
                string? entrada = Console.ReadLine();

                if (int.TryParse(entrada, out int valor))
                    return valor;

                Console.WriteLine("Digite um número inteiro válido.");
            }
        }

        private static int LerCostume(string mensagem)
        {
            while (true)
            {
                Console.Write(mensagem);
                string? entrada = Console.ReadLine();

                if (int.TryParse(entrada, out int valor))
                {
                    bool valido = valor >= -3 && valor <= 3 && valor != -1 && valor != 0;
                    if (valido)
                        return valor;
                }

                Console.WriteLine("Valor inválido. Costume deve ser: -3, -2, 1, 2 ou 3.");
            }
        }

        private static int LerStatus(string mensagem)
        {
            while (true)
            {
                Console.Write(mensagem);
                string? entrada = Console.ReadLine();

                if (int.TryParse(entrada, out int valor))
                {
                    if (valor >= -5 && valor <= 5)
                        return valor;
                }

                Console.WriteLine("Valor inválido. Status deve estar entre -5 e 5.");
            }
        }

        private static int LerPericia(string mensagem)
        {
            while (true)
            {
                Console.Write(mensagem);
                string? entrada = Console.ReadLine();

                if (int.TryParse(entrada, out int valor))
                {
                    bool valido = valor >= -3 && valor <= 3 && valor != 0;
                    if (valido)
                        return valor;
                }

                Console.WriteLine("Valor inválido. Perícia deve estar entre -3 e 3, sem permitir 0.");
            }
        }

        private static void Pausar()
        {
            Console.WriteLine("\nPressione ENTER para continuar...");
            Console.ReadLine();
        }
    }
}