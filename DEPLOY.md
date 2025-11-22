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
- **Backend**: Railway (gratuito) - Root Directory: `Back`

## 🎨 Deploy do Frontend (Vercel)

### Passo 1: Preparar o Frontend

O frontend já está configurado para usar variáveis de ambiente. Você precisará configurar a URL do backend após fazer o deploy.

### Passo 2: Deploy no Vercel

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

5. **NÃO adicione variáveis de ambiente ainda** (faremos isso após o deploy do backend)

6. **Clique em "Deploy"**

7. **Aguarde o deploy completar** e copie a URL gerada (ex: `https://seu-projeto.vercel.app`)

### Passo 3: Configurar Variável de Ambiente (Após Deploy do Backend)

Após fazer o deploy do backend no Railway e obter a URL, volte ao Vercel:

1. Vá em **Settings** → **Environment Variables**
2. Clique em **"Add New"**
3. Adicione:
   - **Name**: `VITE_API_BASE_URL`
   - **Value**: URL do backend do Railway + `/api`
     - Exemplo: `https://seu-backend.railway.app/api`
4. Selecione **"Production"**, **"Preview"** e **"Development"**
5. Clique em **"Save"**
6. Vá em **Deployments** → Clique nos três pontos do último deploy → **"Redeploy"**

## 🔧 Deploy do Backend (Railway)

### Passo 1: Preparar o Backend

O backend já está configurado. Você só precisa garantir que o CORS aceite a URL do Vercel.

### Passo 2: Deploy no Railway

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

   **Opção A: Usar Nixpacks (Recomendado - detecção automática)**
   - Vá em **Settings** → **Service Source**
   - Selecione **"Nixpacks"**
   - ⚠️ **IMPORTANTE**: Certifique-se de que o **Root Directory** está configurado como `Back`
   - O Railway deve detectar automaticamente o projeto .NET
   
   **Opção B: Usar Dockerfile (se tiver problemas com Nixpacks)**
   - Você precisaria criar um Dockerfile na pasta `Back`
   - Mas como removemos Docker, use a Opção A

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

### Passo 3: Configurar CORS no Backend

Após obter a URL do Vercel, você precisa atualizar o CORS no backend:

1. **No Railway**, vá em **Variables**
2. Adicione:
   - **Name**: `FRONTEND_URL`
   - **Value**: URL do seu frontend no Vercel
     - Exemplo: `https://seu-projeto.vercel.app`
3. **Atualize o `Back/Program.cs`** para usar essa variável:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = new List<string>
        {
            "http://localhost:5173",
            "http://localhost:5175",
            "http://localhost:5176",
            "http://localhost:3000"
        };
        
        // Adicionar origem do Vercel se estiver configurada
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            allowedOrigins.Add(frontendUrl);
        }
        
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("X-User-Id");
    });
});
```

4. **Faça commit e push** para o GitHub
5. **Railway fará deploy automático** com as novas configurações

## 🔗 Ordem de Deploy Recomendada

1. **Primeiro**: Deploy do Backend no Railway
   - Obtenha a URL do backend
   - Exemplo: `https://seu-backend.railway.app`

2. **Segundo**: Deploy do Frontend no Vercel
   - Configure a variável `VITE_API_BASE_URL` com a URL do backend
   - Exemplo: `https://seu-backend.railway.app/api`

3. **Terceiro**: Atualizar CORS no Backend
   - Adicione a URL do Vercel nas variáveis de ambiente do Railway
   - Atualize o código se necessário
   - Faça redeploy

## 📝 Checklist de Deploy

### Backend (Railway)
- [ ] Repositório conectado ao Railway (mesmo repositório do frontend)
- [ ] ⚠️ **Root Directory configurado**: `Back` (muito importante!)
- [ ] Build e Start commands configurados automaticamente
- [ ] Variáveis de ambiente configuradas:
  - [ ] `ASPNETCORE_ENVIRONMENT`: `Production`
  - [ ] `ASPNETCORE_URLS`: `http://+:${PORT}`
- [ ] Deploy realizado com sucesso
- [ ] API acessível via URL do Railway
- [ ] URL do backend copiada (ex: `https://seu-backend.railway.app`)

### Frontend (Vercel)
- [ ] Repositório conectado ao Vercel (mesmo repositório do backend)
- [ ] ⚠️ **Root Directory configurado**: `Font/FortniteFront` (muito importante!)
- [ ] Variável `VITE_API_BASE_URL` configurada com URL do backend
  - Formato: `https://seu-backend.railway.app/api`
- [ ] Deploy realizado com sucesso
- [ ] Aplicação acessível via URL do Vercel
- [ ] URL do frontend copiada (ex: `https://seu-projeto.vercel.app`)

### Integração
- [ ] Frontend configurado para usar URL do backend
- [ ] CORS configurado no backend para aceitar URL do Vercel
- [ ] Testes de login/registro funcionando
- [ ] Testes de compra/devolução funcionando

## 🐛 Troubleshooting

### Erro de CORS
- Verifique se a URL do frontend está nas origens permitidas do backend
- Certifique-se de que a variável `FRONTEND_URL` está configurada no Railway
- Verifique se o código do CORS foi atualizado para usar a variável de ambiente

### Erro 404 no Frontend
- Verifique se o `vercel.json` está configurado corretamente (se existir)
- Certifique-se de que o build está gerando a pasta `dist`

### Backend não inicia no Railway
- Verifique os logs no Railway
- Certifique-se de que a porta está configurada corretamente
- Verifique se o banco de dados SQLite está sendo criado
- Verifique se o Root Directory está configurado como `Back`

### Erro Railway: "Railpack could not determine how to build the app"
- ⚠️ **Verifique o Root Directory!** Deve estar configurado como `Back`
- No Railway: Settings → Root Directory → `Back`
- Se ainda não funcionar, delete o serviço e crie novamente, configurando o Root Directory ANTES do primeiro deploy

### Variáveis de ambiente não funcionam
- No Vercel: Settings → Environment Variables → Redeploy
- No Railway: Variables → Redeploy
- Certifique-se de que as variáveis estão configuradas para o ambiente correto (Production/Preview/Development)

### Erro: "Cannot find project file"
- ⚠️ **Verifique o Root Directory!** Deve ser `Back` para backend e `Font/FortniteFront` para frontend
- No Vercel: Settings → General → Root Directory
- No Railway: Settings → Root Directory

## 🔄 Atualizações

Para atualizar a aplicação após mudanças:

1. **Faça commit e push** para o GitHub
2. **Vercel**: Deploy automático (ou manual via dashboard)
3. **Railway**: Deploy automático (ou manual via dashboard)

## 📚 Links Úteis

- [Documentação Vercel](https://vercel.com/docs)
- [Documentação Railway](https://docs.railway.app)
- [Guia de Root Directory no Vercel](https://vercel.com/docs/projects/configuration#root-directory)
- [Guia de Root Directory no Railway](https://docs.railway.app/develop/variables#root-directory)

## 💡 Dica Final

**Use o mesmo repositório!** É mais simples e prático. Apenas certifique-se de configurar o **Root Directory** corretamente em cada serviço:
- Vercel: `Font/FortniteFront`
- Railway: `Back`
