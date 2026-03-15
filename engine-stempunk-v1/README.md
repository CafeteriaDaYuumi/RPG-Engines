# Stempunk RPG Engine

Sistema de gerenciamento e resolução de testes para RPG de mesa baseado em potencial, status, perícias e equipamentos.

O projeto foi desenvolvido em **C# (.NET)** e roda como uma **aplicação de console**.


## Objetivo

Criar uma engine simples para:

- Gerenciar personagens
- Criar equipamentos
- Executar testes de habilidade
- Resolver confrontos entre personagens
- Persistir dados em JSON


## Funcionalidades

- Cadastro de personagens
- Cadastro de equipamentos
- Sistema de status e perícias
- Sistema de potencial
- Execução de testes
- Confronto entre personagens
- Edição e exclusão de dados
- Persistência em JSON


## Sistema do RPG

O sistema utiliza:

- D20
- Modificadores de status
- Modificadores de perícia
- Equipamentos
- Conversão para percentual
- Cálculo de potencial final


### Fórmula base

Resultado Final = Potencial Usado × (Total no Dado × 0.05)


### Versão 1.0.0
Essa versão e um conceito ainda não finalizado, mas já é possível criar personagens, equipamentos e realizar testes básicos.


## Limitações do sistema

O sistema foi projetado para ser flexível e permitir liberdade ao mestre e aos jogadores. Porém, essa liberdade também pode gerar algumas situações incoerentes se não houver interpretação narrativa adequada.

Alguns exemplos:

- Um jogador pode tentar utilizar uma ferramenta ou invenção em um teste onde ela não faria sentido narrativo (por exemplo, utilizar um martelo em um teste de velocidade).
- Equipamentos podem aumentar o potencial de uma ação de forma muito elevada, gerando resultados excessivamente explosivos dependendo da combinação de bônus.

Essas situações não são bloqueadas automaticamente pelo sistema, pois a engine assume que o **mestre atua como moderador narrativo**, decidindo quando o uso de um item ou ação é coerente dentro do contexto da história.

Em outras palavras, o sistema prioriza **flexibilidade e interpretação de roleplay**, deixando parte do controle nas mãos do mestre.

