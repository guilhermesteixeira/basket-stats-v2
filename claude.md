# Basket Stats - Guia de IA

## Visão Geral do Projeto

**Basket Stats** é um projeto para análise e visualização de estatísticas de basquete.

### Objetivos
- Coletar dados de jogadores e equipes
- Calcular e armazenar estatísticas
- Fornecer visualizações e relatórios

---

## Diretrizes para Assistentes de IA

### Quando Precisar de Clarificação
- Escopo de features: quais funcionalidades estão incluídas?
- Decisões de design: qual abordagem preferível?
- Estrutura de dados: como os dados devem ser organizados?
- Dependências externas: quais bibliotecas/APIs usar?

### Padrões de Código
- Manter código simples e legível
- Adicionar comentários apenas quando necessário
- Usar nomenclatura descritiva
- Seguir convenções do projeto

### Workflow
1. **Explorar**: Entender a estrutura e contexto
2. **Planejar**: Criar um plano estruturado (se necessário)
3. **Implementar**: Fazer mudanças mínimas e precisas
4. **Validar**: Testar mudanças para garantir que funcionam

### Checklist para Mudanças
- [ ] Mudanças são mínimas e focadas?
- [ ] Código segue o padrão do projeto?
- [ ] Testes passam (se houver)?
- [ ] Documentação foi atualizada?

---

## Estrutura do Projeto

```
basket-stats/
├── claude.md                # Este arquivo
├── src/                     # Código fonte (quando criado)
├── tests/                   # Testes (quando criado)
├── docs/                    # Documentação (quando criado)
└── package.json             # Metadados do projeto (quando criado)
```

---

## Stack Tecnológico

### Backend
- **Runtime**: Node.js / TypeScript
- **Framework**: Express.js ou Cloud Functions
- **Autenticação**: Keycloak (OpenID Connect)
- **Infraestrutura**: Google Cloud Platform (Serverless)
  - Cloud Functions
  - Cloud Firestore / Datastore
  - Cloud Pub/Sub (para eventos)

---

## MVP - Requisitos

### Funcionalidades Principais
1. **Gestão de Partidas**
   - Receber dados de novas partidas
   - Atualizar eventos em tempo real (pontos, faltas, substituições)
   - Persistir histórico de partidas

2. **Autenticação e Autorização**
   - Integração com Keycloak
   - Login via OpenID Connect
   - Autorização baseada em roles (admin, criador de partida, visualizador)

3. **API REST**
   - POST `/matches` - criar partida
   - PUT `/matches/{id}/events` - adicionar eventos
   - GET `/matches/{id}` - obter detalhes
   - GET `/matches` - listar partidas

### Estrutura de Dados
- **Match**: id, teams, players, startTime, status, events[]
- **Event**: timestamp, type (score, foul, substitution), details
- **User**: id, email, keycloakId, roles[]

### Infraestrutura
- Deploy em Google Cloud Functions
- Banco de dados: Firestore (NoSQL)
- Pubsub para eventos em tempo real (future)
- Variáveis de ambiente via Cloud Secret Manager

---

## Próximas Etapas
1. Configurar projeto GCP
2. Implementar base de autenticação com Keycloak
3. Criar API REST básica
4. Implementar funcionalidade de partidas

---

## Contato & Notas

- Linguagem de Comunicação: Português (PT-BR)
- Foco: Qualidade e clareza no código
