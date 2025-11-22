# 🚀 Guia de Deploy

Este guia explica como fazer deploy da aplicação Fortnite Cosmetics Store em serviços gratuitos.

## ❓ Preciso Separar em Repositórios Diferentes?

**NÃO!** Você pode usar o mesmo repositório (monorepo). Ambos os serviços suportam **Root Directory**, permitindo que você especifique qual pasta usar dentro do mesmo repositório.

### ✅ Vantagens do Monorepo:
- ✅ Um único repositório para gerenciar
- ✅ Histórico de commits unificado
- ✅ Fácil sincronização entre frontend e backend
- ✅ Menos configuração

### 📦 Estrutura de Deploy

- **Frontend**: Vercel (gratuito) - Root Directory: `Font/FortniteFront`
- **Backend**: Railway ou Render (gratuito) - Root Directory: `Back`

## 🎨 Deploy do Frontend (Vercel)

### Opção 1: Via Interface Web (Recomendado)

1. **Acesse [Vercel](https://vercel.com)** e faça login com GitHub
2. **Clique em "Add New Project"**
3. **Importe o repositório** do GitHub (o mesmo repositório que contém frontend e backend)
4. **Configure o projeto:**
   - ⚠️ **IMPORTANTE**: Clique em "Configure Project" antes de fazer deploy
   - **Root Directory**: `Font/FortniteFront` ← Configure isso!
   - **Framework Preset**: Vite (ou deixe auto-detect)
   - **Build Command**: `npm run build` (já vem preenchido)
   - **Output Directory**: `dist` (já vem preenchido)
   - **Install Command**: `npm install` (já vem preenchido)

5. **Adicione variáveis de ambiente:**
   - `VITE_API_BASE_URL`: URL do backend (será configurada após deploy do backend)
     - Exemplo: `https://seu-backend.railway.app/api` ou `https://seu-backend.onrender.com/api`

6. **Clique em "Deploy"**

### Opção 2: Via CLI

```bash
# Instalar Vercel CLI
npm i -g vercel

# Navegar para a pasta do frontend
cd Font/FortniteFront

# Fazer login
vercel login

# Deploy (o Vercel detectará automaticamente o vercel.json)
vercel

# Adicionar variável de ambiente
vercel env add VITE_API_BASE_URL
# Digite a URL do backend quando solicitado
```

### Configuração Pós-Deploy

Após o deploy do backend, atualize a variável de ambiente `VITE_API_BASE_URL` no Vercel:
1. Vá em **Settings** → **Environment Variables**
2. Edite `VITE_API_BASE_URL` com a URL do backend
3. Faça um novo deploy (ou aguarde o redeploy automático)

## 🔧 Deploy do Backend

### Opção 1: Railway (Recomendado)

1. **Acesse [Railway](https://railway.app)** e faça login com GitHub
2. **Clique em "New Project"** → **"Deploy from GitHub repo"**
3. **Selecione o repositório** e a branch (o mesmo repositório do frontend)
4. **⚠️ CONFIGURE O ROOT DIRECTORY ANTES DE QUALQUER COISA:**
   - Após selecionar o repositório, **NÃO clique em Deploy ainda**
   - Clique em **"Settings"** (ou "Configure")
   - Procure por **"Root Directory"** ou **"Working Directory"**
   - Digite: `Back` ← **MUITO IMPORTANTE!**
   - Salve as configurações

5. **Escolha o método de build:**

   **Opção A: Usar Dockerfile (Recomendado - mais confiável)**
   - Vá em **Settings** → **Service Source**
   - Selecione **"Dockerfile"**
   - O Railway usará o `Dockerfile` que está na pasta `Back`
   
   **Opção B: Usar Nixpacks (detecção automática)**
   - Vá em **Settings** → **Service Source**
   - Selecione **"Nixpacks"**
   - O Railway deve detectar automaticamente o projeto .NET
   - Se não detectar, use a Opção A (Dockerfile)

6. **Adicione variáveis de ambiente:**
   - Vá em **Variables**
   - Adicione:
     - `ASPNETCORE_ENVIRONMENT`: `Production`
     - `ASPNETCORE_URLS`: `http://+:${PORT}` (Railway define PORT automaticamente)
     - `PORT`: Deixe Railway definir automaticamente (não precisa adicionar manualmente)

7. **Agora sim, faça o deploy:**
   - Clique em **"Deploy"** ou aguarde o deploy automático
   - Aguarde o build completar
   - Copie a URL gerada (ex: `https://seu-projeto.railway.app`)

### Opção 2: Render

1. **Acesse [Render](https://render.com)** e faça login com GitHub
2. **Clique em "New +"** → **"Web Service"**
3. **Conecte o repositório** do GitHub (o mesmo repositório do frontend)
4. **Configure o serviço:**
   - **Name**: `fortnite-backend`
   - ⚠️ **Root Directory**: `Back` ← Configure isso!
   - **Environment**: `Docker` ou `.NET`
   - **Build Command**: `dotnet publish -c Release -o ./publish`
   - **Start Command**: `dotnet ./publish/Backend.dll`
   - **Plan**: Free

5. **Adicione variáveis de ambiente:**
   - `ASPNETCORE_ENVIRONMENT`: `Production`
   - `ASPNETCORE_URLS`: `http://+:10000`

6. **Clique em "Create Web Service"**

### Opção 3: Fly.io

1. **Instale o Fly CLI:**
```bash
# Windows (PowerShell)
iwr https://fly.io/install.ps1 -useb | iex

# Mac/Linux
curl -L https://fly.io/install.sh | sh
```

2. **Faça login:**
```bash
fly auth login
```

3. **Navegue para a pasta do backend:**
```bash
cd Back
```

4. **Inicialize o projeto:**
```bash
fly launch
```

5. **Configure o `fly.toml`** (será criado automaticamente)

6. **Deploy:**
```bash
fly deploy
```

## 🔗 Configuração de CORS

O backend já está configurado para aceitar qualquer origem em produção automaticamente. Se quiser restringir a origens específicas:

1. **No Railway/Render**, adicione a variável de ambiente:
   - `FRONTEND_URL`: URL do seu frontend no Vercel
     - Exemplo: `https://seu-projeto.vercel.app`

2. **O `Program.cs`** já está configurado para usar essa variável automaticamente

## 📝 Checklist de Deploy

### Frontend (Vercel)
- [ ] Repositório conectado ao Vercel (mesmo repositório do backend)
- [ ] ⚠️ **Root Directory configurado**: `Font/FortniteFront` (muito importante!)
- [ ] Variável `VITE_API_BASE_URL` configurada com URL do backend
- [ ] Deploy realizado com sucesso
- [ ] Aplicação acessível via URL do Vercel

### Backend (Railway/Render)
- [ ] Repositório conectado (mesmo repositório do frontend)
- [ ] ⚠️ **Root Directory configurado**: `Back` (muito importante!)
- [ ] Build e Start commands configurados
- [ ] Variáveis de ambiente configuradas
- [ ] Deploy realizado com sucesso
- [ ] API acessível via URL do serviço
- [ ] Swagger acessível (se habilitado)

### Integração
- [ ] Frontend configurado para usar URL do backend
- [ ] CORS configurado no backend
- [ ] Testes de login/registro funcionando
- [ ] Testes de compra/devolução funcionando

## 🐛 Troubleshooting

### Erro de CORS
- Verifique se a URL do frontend está nas origens permitidas do backend
- Em produção, o backend está configurado para aceitar qualquer origem

### Erro 404 no Frontend
- Verifique se o `vercel.json` está configurado corretamente
- Certifique-se de que o build está gerando a pasta `dist`

### Backend não inicia
- Verifique os logs no Railway/Render
- Certifique-se de que a porta está configurada corretamente
- Verifique se o banco de dados SQLite está sendo criado

### Variáveis de ambiente não funcionam
- No Vercel: Settings → Environment Variables → Redeploy
- No Railway: Variables → Redeploy
- No Render: Environment → Save Changes → Manual Deploy

### Erro: "Cannot find project file"
- ⚠️ **Verifique o Root Directory!** Deve ser `Back` para backend e `Font/FortniteFront` para frontend
- No Vercel: Settings → General → Root Directory
- No Railway: Settings → Root Directory
- No Render: Settings → Root Directory

### Erro Railway: "Railpack could not determine how to build the app"

Este erro acontece quando o Railway não encontra o arquivo `Backend.csproj` porque está analisando a raiz do repositório.

**Solução (PASSO A PASSO):**

1. **Configure o Root Directory PRIMEIRO:**
   - No Railway, vá em **Settings** → **Root Directory**
   - Digite: `Back`
   - Salve

2. **Escolha o método de build:**
   - Vá em **Settings** → **Service Source**
   - Selecione **"Dockerfile"** (mais confiável)
   - Ou selecione **"Nixpacks"** se preferir detecção automática

3. **Se ainda não funcionar:**
   - Delete o serviço e crie novamente
   - Desta vez, configure o Root Directory **ANTES** de fazer o primeiro deploy
   - Use o Dockerfile como método de build

4. **Verifique se os arquivos estão corretos:**
   - O arquivo `Backend.csproj` deve estar em `Back/Backend.csproj`
   - O arquivo `Dockerfile` deve estar em `Back/Dockerfile`
   - O arquivo `Program.cs` deve estar em `Back/Program.cs`

## 🔄 Atualizações

Para atualizar a aplicação após mudanças:

1. **Faça commit e push** para o GitHub
2. **Vercel**: Deploy automático (ou manual via dashboard)
3. **Railway/Render**: Deploy automático (ou manual via dashboard)

## 📚 Links Úteis

- [Documentação Vercel](https://vercel.com/docs)
- [Documentação Railway](https://docs.railway.app)
- [Documentação Render](https://render.com/docs)
- [Documentação Fly.io](https://fly.io/docs)

## 💡 Dica Final

**Use o mesmo repositório!** É mais simples e prático. Apenas certifique-se de configurar o **Root Directory** corretamente em cada serviço:
- Vercel: `Font/FortniteFront`
- Railway/Render: `Back`
