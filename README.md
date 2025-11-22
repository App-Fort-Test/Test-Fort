# Fortnite Cosmetics Store

Sistema de loja de cosméticos do Fortnite com gerenciamento de usuários, transações e inventário.

## 🌐 Deploy Online

A aplicação pode ser implantada gratuitamente:
- **Frontend**: [Vercel](https://vercel.com) (gratuito)
- **Backend**: [Railway](https://railway.app) (gratuito)

📖 **Veja o guia completo de deploy em [DEPLOY.md](./DEPLOY.md)**

## 📋 Instruções para Rodar o Projeto Localmente

### Pré-requisitos

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** e **npm** - [Download](https://nodejs.org/)
- **Git** - [Download](https://git-scm.com/)

### Backend (API)

1. Navegue até a pasta do backend:
```bash
cd Back
```

2. Restaure as dependências (se necessário):
```bash
dotnet restore
```

3. Execute o projeto:

**Opção A: Usando dotnet run (desenvolvimento)**
```bash
dotnet run
```

**Opção B: Compilar e executar diretamente (sem dotnet run)**
```bash
# Windows (PowerShell)
.\start-backend.ps1

# Windows (CMD)
start-backend.bat

# Linux/Mac
chmod +x start-backend.sh
./start-backend.sh
```

**Opção C: Compilar manualmente e executar**
```bash
dotnet build -c Release
.\bin\Release\net8.0\Backend.exe  # Windows
./bin/Release/net8.0/Backend       # Linux/Mac
```

O backend estará disponível em:
- **API**: `http://localhost:5155`
- **Swagger UI**: `http://localhost:5155/swagger` (apenas em desenvolvimento)

### Frontend (Vue.js)

1. Navegue até a pasta do frontend:
```bash
cd Font/FortniteFront
```

2. Instale as dependências:
```bash
npm install
```

3. Execute o servidor de desenvolvimento:
```bash
npm run dev
```

O frontend estará disponível em:
- **Aplicação**: `http://localhost:5173` (ou outra porta disponível)

### Configuração de CORS

O backend está configurado para aceitar requisições das seguintes origens:
- `http://localhost:5173`
- `http://localhost:5175`
- `http://localhost:5176`
- `http://localhost:3000`

Se você estiver usando uma porta diferente, edite o arquivo `Back/Program.cs` e adicione sua porta na configuração de CORS.

### Banco de Dados

O banco de dados SQLite (`fortnite.db`) é criado automaticamente na primeira execução do backend. Não é necessária configuração adicional.

## 🛠 Tecnologias Utilizadas

### Backend

- **.NET 8.0** - Framework principal
- **ASP.NET Core** - Framework web
- **Entity Framework Core 8.0** - ORM para acesso a dados
- **SQLite** - Banco de dados relacional
- **Swagger/OpenAPI** - Documentação da API
- **SHA256** - Hash de senhas

### Frontend

- **Vue.js 3.5** - Framework JavaScript reativo
- **Vite 7.2** - Build tool e servidor de desenvolvimento
- **Axios 1.13** - Cliente HTTP para requisições à API
- **Composition API** - API de composição do Vue 3

### APIs Externas

- **Fortnite API v2** - API pública para dados de cosméticos do Fortnite
  - Endpoint: `https://fortnite-api.com/v2/`

## 🏗 Decisões Técnicas Relevantes

### Arquitetura

1. **Separação Frontend/Backend**
   - Frontend e backend são projetos independentes
   - Comunicação via REST API
   - Facilita deploy e manutenção independente

2. **Padrão MVC no Backend**
   - Controllers para endpoints da API
   - Services para lógica de negócio
   - Models para entidades do domínio
   - Data Access Layer com Entity Framework

3. **Composition API no Frontend**
   - Uso de composables (`useAuth`, `useCosmetics`, `useTransactions`)
   - Reutilização de lógica entre componentes
   - Melhor organização e testabilidade

### Banco de Dados

1. **SQLite**
   - Escolhido por simplicidade e não requer servidor separado
   - Adequado para desenvolvimento e pequenos projetos
   - Fácil backup (arquivo único)

2. **Sistema de Transações**
   - Inventário calculado dinamicamente a partir de transações
   - Permite histórico completo de compras e devoluções
   - Facilita auditoria e rastreabilidade

3. **Índices e Constraints**
   - Índices únicos em Email e Username
   - Índice composto em UserId + CosmeticId para evitar duplicatas
   - Relacionamentos com cascade delete

### Autenticação e Segurança

1. **Hash de Senhas**
   - SHA256 para hash de senhas
   - Senhas nunca armazenadas em texto plano
   - Verificação de hash na autenticação

2. **Autenticação via Header**
   - Uso do header `X-User-Id` para identificar usuário
   - Não utiliza tokens JWT (simplificado para este projeto)
   - Autenticação baseada em sessão do frontend

### Performance e Cache

1. **Cache em Múltiplas Camadas**
   - Cache em memória (Map) para páginas recentes
   - Cache no localStorage do navegador
   - Cache com expiração (30 minutos para cosméticos, 5 minutos para shop)

2. **Paginação**
   - Paginação no backend e frontend
   - Pré-carregamento de páginas adjacentes
   - Reduz carga inicial e melhora performance

3. **Lazy Loading**
   - Carregamento sob demanda de dados externos
   - Enriquecimento de cosméticos apenas quando necessário
   - Reduz requisições desnecessárias

### Gerenciamento de Estado

1. **Composables Reativos**
   - Estado global gerenciado via composables Vue
   - Reatividade automática com `ref` e `reactive`
   - Sincronização entre componentes

2. **Eventos Customizados**
   - Sistema de eventos para atualização de transações
   - Polling como fallback para atualizações em tempo real
   - Comunicação desacoplada entre componentes

### UX/UI

1. **Feedback Visual Imediato**
   - Atualização otimista do estado após compras/devoluções
   - Atualização da wallet em tempo real
   - Badges e botões atualizados instantaneamente

2. **Filtros e Busca**
   - Filtros combináveis (tipo, raridade, preço, data)
   - Filtro "possuído" apenas para usuários logados
   - Busca por nome com debounce implícito

3. **Paginação Inteligente**
   - Pré-carregamento de páginas próximas
   - Cache de páginas visitadas
   - Navegação fluida entre páginas

### Integração com API Externa

1. **Enriquecimento de Dados**
   - Dados básicos do backend
   - Enriquecimento com dados da Fortnite API
   - Fallback gracioso se API externa falhar

2. **Tratamento de Erros**
   - Tratamento de erros de rede
   - Retry automático em alguns casos
   - Mensagens de erro amigáveis ao usuário

### Estrutura de Pastas

```
Back/
├── Controllers/     # Endpoints da API
├── Services/         # Lógica de negócio
├── Models/          # Entidades do domínio
├── Data/            # Contexto do Entity Framework
└── Program.cs       # Configuração da aplicação

Font/FortniteFront/
├── src/
│   ├── components/  # Componentes Vue
│   ├── composables/ # Lógica reutilizável
│   ├── services/     # Serviços de API
│   └── App.vue      # Componente raiz
└── package.json
```

## 📝 Notas Adicionais

- O banco de dados SQLite é criado automaticamente na primeira execução
- Usuários novos recebem 10.000 V-Bucks iniciais
- O sistema calcula o inventário dinamicamente a partir das transações
- Cache é limpo automaticamente após compras/devoluções para garantir dados atualizados
- Swagger está disponível apenas em ambiente de desenvolvimento

## 🔧 Troubleshooting

### Erro de CORS
Se encontrar erros de CORS, verifique se a porta do frontend está na lista de origens permitidas em `Back/Program.cs`.

### Banco de dados bloqueado
Se o SQLite estiver bloqueado, certifique-se de que não há outras instâncias do backend rodando.

### Porta já em uso
Se a porta 5155 estiver em uso, você pode alterar no arquivo `Back/Properties/launchSettings.json`.

### Processo Backend bloqueando build
Se o build falhar porque o arquivo `Backend.exe` está em uso:
1. Pare o processo Backend rodando
2. Use o script: `cd Back; .\kill-backend.ps1`
3. Ou pare manualmente via Gerenciador de Tarefas
