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
   - **Project Name**: Use apenas letras, números e underscores
     - ❌ **NÃO use**: hífens (`test-fort`), espaços, ou começar com número
     - ✅ **Use**: `test_fort`, `testfort`, `fortnite_front`, etc.
   - **Root Directory**: `Font/FortniteFront` ← Configure isso!
   - **Framework Preset**: Vite (ou deixe auto-detect)
   - **Build Command**: `npm run build` (já vem preenchido)
   - **Output Directory**: `dist` (já vem preenchido)
   - **Install Command**: `npm install` (já vem preenchido)

5. **NÃO adicione variáveis de ambiente ainda** (faremos isso após o deploy do backend)

6. **Clique em "Deploy"**

7. **Aguarde o deploy completar** e copie a URL gerada (ex: `https://seu-projeto.vercel.app`)

### Passo 3: Configurar Variável de Ambiente (Após Deploy do Backend)

Após fazer o deploy do backend no Railway e obter a URL, configure no Vercel:

**Durante a criação do projeto (ou depois em Settings):**

1. Na seção **"Environment Variables"** (ou **"Add Environment Variable"**)
2. Adicione:
   - **Key/Name**: `VITE_API_BASE_URL`
   - **Value**: `https://helpful-friendship-production-7f08.up.railway.app/api`
     - ⚠️ **IMPORTANTE**: 
       - Deve começar com `https://`
       - Deve terminar com `/api`
       - Formato completo: `https://sua-url-railway.app/api`
       - ❌ **NÃO use**: apenas o domínio sem `https://` e `/api`
       - ❌ **NÃO use**: `helpful-friendship-production-7f08.up.railway.app` (sem protocolo e path)
       - ✅ **Use**: `https://helpful-friendship-production-7f08.up.railway.app/api`
     - 💡 **Nota**: O código agora normaliza automaticamente a URL, mas é melhor configurar corretamente
3. Selecione **"Production"**, **"Preview"** e **"Development"** (ou apenas Production)
4. Clique em **"Add"** ou **"Save"**

**Se já criou o projeto:**
1. Vá em **Settings** → **Environment Variables**
2. Clique em **"Add New"**
3. Adicione a mesma variável acima
4. Vá em **Deployments** → Clique nos três pontos do último deploy → **"Redeploy"**

## 🔧 Deploy do Backend (Railway)

### Passo 1: Preparar o Backend

O backend já está configurado. Você só precisa garantir que o CORS aceite a URL do Vercel.

### Passo 2: Deploy no Railway

1. **Acesse [Railway](https://railway.app)** e faça login com GitHub

2. **Clique em "New Project"** → **"Deploy from GitHub repo"**

3. **Selecione o repositório** e a branch (o mesmo repositório do frontend)

4. **⚠️ CONFIGURE O ROOT DIRECTORY ANTES DE QUALQUER COISA:**
   - Após selecionar o repositório, **NÃO clique em Deploy ainda**
   - Na seção **"Source Repo"**, clique em **"Add Root Directory"**
   - Digite: `Back` ← **MUITO IMPORTANTE!**
   - Clique em **"Update"** para salvar

5. **Configure o Builder (Build) - ⚠️ MUITO IMPORTANTE:**
   - Na seção **"Build"**, procure por **"Builder"** ou **"Build Method"**
   - **MUDE para "Dockerfile"** (NÃO deixe como "Nixpacks" ou "Auto-detect")
   - ⚠️ **Se estiver "Nixpacks"**: Clique e mude para **"Dockerfile"**
   - Em **"Dockerfile Path"**, deixe **VAZIO** (não digite nada)
     - ⚠️ **Nota**: Com Root Directory = `Back`, o Railway procura o Dockerfile dentro dessa pasta automaticamente
     - ❌ **NÃO use**: `Back/Dockerfile` ou `Dockerfile` (isso pode causar problemas)
   - Em **"Watch Paths"**, adicione: `/Back/**` (para fazer deploy quando houver mudanças na pasta Back)
   - **Salve as configurações** antes de fazer deploy

6. **Configure o Deploy:**
   - Na seção **"Deploy"**, em **"Custom Start Command"**, **DEIXE VAZIO**
     - ⚠️ **Importante**: O Dockerfile já define o comando de start via `ENTRYPOINT`, não precisa configurar aqui

7. **⚠️ LIMPE O CACHE DO RAILWAY (se estiver usando Dockerfile antigo):**
   - Vá em **Settings** → **Deploy**
   - Procure por **"Clear Build Cache"** ou **"Clear Cache"**
   - Clique para limpar o cache
   - Isso força o Railway a usar o Dockerfile atualizado do repositório

8. **⚠️ Configure Volume Persistente (IMPORTANTE para SQLite):**
   - Vá em **Settings** → **Volumes**
   - Clique em **"Add Volume"**
   - Configure:
     - **Mount Path**: `/data`
   - Isso garante que o banco SQLite não seja perdido entre rebuilds
   - ⚠️ **Sem volume**: Todos os dados serão perdidos quando o Railway fizer rebuild do container

9. **Adicione variáveis de ambiente:**
   - Vá em **Variables**
   - Adicione:
     - `ASPNETCORE_ENVIRONMENT`: `Production`
     - `ASPNETCORE_URLS`: `http://+:${PORT}` (Railway define PORT automaticamente)
     - `RAILWAY_VOLUME_MOUNT_PATH`: `/data` (para usar o volume persistente criado acima)
     - `PORT`: Deixe Railway definir automaticamente (não precisa adicionar manualmente)

10. **Agora sim, faça o deploy:**
   - Clique em **"Deploy"** ou aguarde o deploy automático
   - Aguarde o build completar

10. **Obtenha a URL do backend (IMPORTANTE):**
    - Após o deploy bem-sucedido, vá na página principal do serviço no Railway
    - Você verá **"Unexposed service"** (serviço não exposto) - isso significa que não há URL pública ainda
    - Para gerar a URL pública:
      1. Na página do serviço, procure a seção **"Networking"** (pode estar na lateral ou no topo)
      2. Ou vá em **Settings** → **Networking**
      3. Procure por **"Public Networking"** ou **"Generate Domain"**
      4. Clique em **"Generate Domain"** para criar uma URL pública
    - Após gerar, a URL aparecerá na página do serviço (ex: `https://test-fort-production.up.railway.app`)
    - **Copie essa URL completa** (sem `/api` no final)
    - Você usará essa URL no Vercel como: `https://sua-url-railway.app/api`

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
  - [ ] `ASPNETCORE_ENVIRONMENT`: `Production
  - [ ] `ASPNETCORE_URLS`: `http://+:${PORT}`
- [ ] ⚠️ **Volume persistente configurado** (para SQLite não perder dados entre rebuilds)
  - [ ] Volume criado no Railway com Mount Path: `/data`
  - [ ] Variável `RAILWAY_VOLUME_MOUNT_PATH` configurada: `/data`
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

## ⚠️ IMPORTANTE: Persistência de Dados com SQLite no Railway

**PROBLEMA CRÍTICO**: O backend está usando SQLite, e o arquivo `.db` fica dentro do container. **Os dados NÃO persistem entre rebuilds do Railway**.

### O que acontece:
- ✅ O banco funciona normalmente durante a execução
- ❌ **Todos os dados são perdidos** quando o Railway faz rebuild do container
- ❌ Rebuilds acontecem quando você faz push, atualiza configurações, ou o Railway reinicia o serviço

### Soluções Recomendadas:

#### Opção 1: Usar Volume Persistente do Railway (Recomendado)
1. No Railway, vá em **Settings** → **Volumes**
2. Clique em **"Add Volume"**
3. Configure:
   - **Mount Path**: `/data` (ou `/app/data`)
4. Adic`ione variável de ambiente:
   - `RAILWAY_VOLUME_MOUNT_PATH`: `/data`
5. O código já está configurado para usar essa variável automaticamente

#### Opção 2: Migrar para PostgreSQL (Melhor para Produção)
- O Railway oferece PostgreSQL gratuito
- Dados persistem permanentemente
- Melhor performance e escalabilidade
- Requer mudanças no código (trocar SQLite por PostgreSQL)

#### Opção 3: Usar Serviço de Banco de Dados Externo
- Usar um serviço como Supabase, PlanetScale, ou Railway PostgreSQL
- Dados persistem independente do container

### Configuração Atual:
O código já tenta usar `RAILWAY_VOLUME_MOUNT_PATH` ou `/tmp`, mas `/tmp` também é efêmero. Para persistência real, use um Volume do Railway.

## 🐛 Troubleshooting

### Erro: "Dockerfile `Back/Dockerfile` does not exist"
- **Causa**: O Dockerfile não foi commitado no Git ou o Railway não está encontrando
- **Solução**:
  1. Verifique se o Dockerfile existe em `Back/Dockerfile`
  2. Faça commit e push: `git add Back/Dockerfile && git commit -m "Add Dockerfile" && git push`
  3. No Railway, limpe o cache de build
  4. Force um novo deploy

### Erro: "Railpack could not determine how to build the app" ou "No .NET SDKs were found"
- **Causa**: O Railway está usando Nixpacks (geração automática) em vez do Dockerfile
- **Solução URGENTE - CONFIGURE MANUALMENTE NO RAILWAY**:
  
  ⚠️ **IMPORTANTE**: O Railway precisa ser configurado manualmente para usar o Dockerfile
  
  1. **No Railway, vá em Settings → Build** (ou **Settings → Service Source**)
  2. **MUDE o Builder para "Dockerfile"**:
     - Se estiver "Nixpacks", "Railpack" ou "Auto-detect", clique e selecione **"Dockerfile"**
     - Isso é CRÍTICO - o Railway DEVE usar o Dockerfile, não Nixpacks
  3. **Em "Dockerfile Path"**, deixe **VAZIO** (não digite nada)
     - Com Root Directory = `Back`, o Railway encontra o Dockerfile automaticamente
  4. **Verifique o Root Directory**:
     - Vá em **Settings → Source**
     - Certifique-se de que **Root Directory** está como `Back` (sem barra no final)
  5. **Salve as configurações**
  6. **Limpe o cache** (Settings → Deploy → Clear Cache, se disponível)
  7. **Force um novo deploy**:
     - Vá em **Deployments**
     - Clique nos três pontos do último deploy → **"Redeploy"**
     - Ou delete o último deploy e crie um novo
  
  **Nota**: O arquivo `railway.json` ajuda, mas o Railway pode não detectá-lo automaticamente. Configure manualmente para garantir.

### Erro: "Dockerfile `Dockerfile` does not exist"
- **Causa**: O Railway não está encontrando o Dockerfile mesmo com Root Directory configurado
- **Solução**:
  1. **Verifique se o Dockerfile está no repositório**:
     - O arquivo deve estar em `Back/Dockerfile`
     - Deve estar commitado no Git
  2. **Verifique o Root Directory no Railway**:
     - Vá em **Settings → Source**
     - Certifique-se de que **Root Directory** está como `Back` (sem barra no final)
  3. **No railway.json, remova o dockerfilePath** (deixe o Railway detectar automaticamente):
     - O arquivo `Back/railway.json` já está configurado corretamente
  4. **Faça commit e push**:
     ```bash
     git add Back/railway.json
     git commit -m "Ajusta railway.json para detecção automática do Dockerfile"
     git push origin main
     ```
  5. **Force um novo deploy** no Railway

### Erro: "MSB1003: Specify a project or solution file" ou ".NET 6.0" no build
- **Causa**: O Railway está usando Nixpacks (geração automática) ou um Dockerfile em cache antigo
- **Solução COMPLETA**:
  1. **No Railway, vá em Settings → Build**
  2. **Certifique-se de que "Dockerfile" está selecionado** (NÃO "Nixpacks")
     - Se estiver "Nixpacks", mude para "Dockerfile"
  3. **Em "Dockerfile Path"**, deixe **VAZIO** (não use `Back/Dockerfile`)
     - ⚠️ Com Root Directory = `Back`, o Railway procura o Dockerfile dentro dessa pasta automaticamente
  4. **Verifique o Root Directory**:
     - Vá em **Settings → Source**
     - Certifique-se de que **Root Directory** está como `Back`
  5. **Limpe o cache de build**:
     - Vá em **Settings → Deploy**
     - Procure por **"Clear Build Cache"** ou **"Clear Cache"**
     - Clique para limpar
  6. **Force um novo deploy**:
     - Vá em **Deployments**
     - Clique nos três pontos do último deploy → **"Redeploy"**
     - Ou delete o último deploy e crie um novo
  7. **Verifique se o Dockerfile está no repositório**:
     - O arquivo deve estar em `Back/Dockerfile`
     - Deve usar `.NET 8.0` (não `.NET 6.0`)
     - Deve copiar `Backend.csproj` primeiro, depois fazer `dotnet restore`

### Erro 405 (Method Not Allowed) - URL incorreta no frontend
- **Causa**: A variável de ambiente `VITE_API_BASE_URL` está configurada incorretamente no Vercel
- **Sintoma**: A URL da requisição mostra algo como `https://vercel.app/railway.app/auth/register` (URLs concatenadas)
- **Solução**:
  1. **No Vercel, vá em Settings → Environment Variables**
  2. **Edite ou recrie a variável `VITE_API_BASE_URL`**:
     - ❌ **ERRADO**: `helpful-friendship-production-7f08.up.railway.app` (sem protocolo e path)
     - ❌ **ERRADO**: `https://helpful-friendship-production-7f08.up.railway.app` (sem `/api`)
     - ✅ **CORRETO**: `https://helpful-friendship-production-7f08.up.railway.app/api`
  3. **Formato correto**:
     - Deve começar com `https://`
     - Deve terminar com `/api`
     - Exemplo: `https://helpful-friendship-production-7f08.up.railway.app/api`
  4. **Após corrigir, faça redeploy**:
     - Vá em **Deployments**
     - Clique nos três pontos do último deploy → **"Redeploy"**
  5. **Verifique se o backend está funcionando**:
     - Teste a URL diretamente: `https://helpful-friendship-production-7f08.up.railway.app/api/auth/register`
     - Deve retornar 405 para GET (normal), mas confirma que a rota existe

### Erro de CORS
- Verifique se a URL do frontend está nas origens permitidas do backend
- Certifique-se de que a variável `FRONTEND_URL` está configurada no Railway
- Verifique se o código do CORS foi atualizado para usar a variável de ambiente

### Erro Vercel: "Aparentemente, esse erro foi causado pelo aplicativo"
- **Causa**: O build do frontend falhou ou há um erro em runtime
- **Solução**:
  1. **Verifique os logs do deploy no Vercel**:
     - Vá em **Deployments** → Clique no último deploy
     - Veja a aba **"Build Logs"** ou **"Function Logs"**
     - Procure por erros de build ou runtime
  2. **Verifique se a variável de ambiente está configurada**:
     - Vá em **Settings** → **Environment Variables**
     - Certifique-se de que `VITE_API_BASE_URL` está configurada
     - Formato: `https://sua-url-railway.app/api`
  3. **Verifique o Root Directory**:
     - Vá em **Settings** → **General**
     - Certifique-se de que **Root Directory** está como `Font/FortniteFront`
  4. **Teste o build localmente**:
     ```bash
     cd Font/FortniteFront
     npm install
     npm run build
     ```
     - Se o build falhar localmente, corrija os erros antes de fazer deploy
  5. **Verifique se há erros de sintaxe no código**:
     - Procure por imports faltando
     - Verifique se todos os arquivos estão corretos
  6. **Force um novo deploy** após corrigir os problemas

### Erro Vercel: "404: NOT_FOUND - DEPLOYMENT_NOT_FOUND"
- **Causa**: O deploy ainda não foi iniciado ou falhou durante a criação do projeto
- **Solução**:
  1. Verifique se você clicou em **"Deploy"** após configurar o projeto
  2. Vá na página do projeto no Vercel e verifique a aba **"Deployments"**
  3. Se não houver nenhum deploy, clique em **"Redeploy"** ou **"Deploy"**
  4. Verifique se o **Root Directory** está configurado corretamente: `Font/FortniteFront`
  5. Verifique os logs do deploy para ver se há erros de build
  6. Se o projeto não foi criado corretamente, delete e crie novamente

### Erro 404 no Frontend (após deploy)
- Verifique se o `vercel.json` está configurado corretamente (se existir)
- Certifique-se de que o build está gerando a pasta `dist`
- Verifique se a URL está correta (pode ter mudado após o deploy)

### Erro: "Failed to get private network endpoint" (Private Networking)
- **Causa**: Erro ao configurar a rede privada do Railway (não é crítico)
- **Solução**: 
  - Isso **não impede o deploy** - é apenas um aviso
  - Private Networking é usado apenas para comunicação entre serviços Railway
  - Se você só tem um serviço (backend), pode ignorar esse erro
  - Se precisar corrigir: desative e reative o Private Networking nas configurações
  - Ou simplesmente ignore - o serviço público (HTTP) continuará funcionando normalmente

### Erro: Não consegue fazer login - banco de dados não conecta
- **Causa**: Banco SQLite não está sendo criado ou não tem permissão de escrita
- **Sintomas**: Erro ao fazer login, mensagens de "unable to open database" ou "no such table"
- **Solução URGENTE**:
  1. **Configure Volume Persistente** (CRÍTICO):
     - Vá em **Settings → Volumes**
     - Clique em **"Add Volume"**
     - Configure **Mount Path**: `/data`
  2. **Configure Variável de Ambiente**:
     - Vá em **Settings → Variables**
     - Adicione: `RAILWAY_VOLUME_MOUNT_PATH` = `/data`
  3. **Verifique os Logs do Railway**:
     - Vá em **Deployments** → Clique no último deploy → **Logs**
     - Procure por: `=== Iniciando criação do banco de dados ===`
     - Verifique se aparece: `✅ Banco de dados criado/verificado com sucesso!`
     - Se aparecer erro, copie a mensagem completa
  4. **Faça Redeploy** após configurar volume e variável
  5. **O código agora**:
     - Tenta criar o banco na inicialização (5 tentativas com logs detalhados)
     - Verifica se o banco existe antes de cada requisição
     - Tenta criar o banco automaticamente se não existir
  6. **Se ainda não funcionar**:
     - Verifique se o volume está montado corretamente
     - Verifique permissões nos logs
     - Consulte `FIX_DATABASE.md` para diagnóstico detalhado

### Erro de conexão com banco de dados no Railway
- **Causa**: SQLite pode ter problemas de permissão ou o diretório pode ser efêmero no Railway
- **Solução**:
  1. **Verifique os logs do Railway** para ver a mensagem de erro específica
  2. **O código já está configurado** para usar `/tmp` ou diretório persistente se disponível
  3. **No Railway, use um volume persistente** (veja seção acima):
     - Vá em **Settings → Volumes**
     - Crie um volume persistente com Mount Path: `/data`
     - Configure a variável de ambiente `RAILWAY_VOLUME_MOUNT_PATH`: `/data`
  4. **O banco será criado automaticamente** na primeira requisição se houver permissão
  5. **Verifique os logs** para ver onde o banco está sendo criado

### Backend não inicia no Railway
- Verifique os logs no Railway
- Certifique-se de que a porta está configurada corretamente
- Verifique se o banco de dados SQLite está sendo criado
- Verifique se o Root Directory está configurado como `Back`

### Erro Railway: "Railpack could not determine how to build the app" ⚠️ URGENTE

**Sintoma**: Logs mostram:
```
⚠ Script start.sh not found
✖ Railpack could not determine how to build the app.
```

**Causa**: O Railway está usando Railpack/Nixpacks em vez do Dockerfile.

**Solução RÁPIDA**:
1. Acesse: https://railway.com/project/f91c4260-84da-457e-9311-5da58bedc6f9/service/395283a7-0e23-492b-a4e3-a02aebb6fb76/settings
2. Vá em **Settings** → **Build**
3. **MUDE o Builder de "Nixpacks"/"Railpack" para "Dockerfile"**
4. Em **"Dockerfile Path"**, deixe **VAZIO**
5. Verifique **Root Directory** = `Back`
6. Limpe o cache (Settings → Deploy → Clear Cache)
7. Faça **Redeploy**

Veja também: `FIX_RAILPACK_ERROR.md` para instruções detalhadas.

### Erro Railway: "Railpack could not determine how to build the app" (versão antiga)
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
