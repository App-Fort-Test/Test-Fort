# ✅ Checklist de Configuração - Vercel e Railway

Use este checklist para verificar se tudo está configurado corretamente.

## 🎨 Frontend (Vercel)

### Configurações Básicas
- [ ] **Root Directory**: `Font/FortniteFront`
- [ ] **Framework Preset**: Vite (ou auto-detect)
- [ ] **Build Command**: `npm run build`
- [ ] **Output Directory**: `dist`

### Variável de Ambiente (CRÍTICO)
- [ ] **Variável `VITE_API_BASE_URL` configurada**
  - ✅ **Formato correto**: `https://sua-url-railway.app/api`
  - ❌ **Formato errado**: `sua-url-railway.app` (sem `https://` e `/api`)
  - ❌ **Formato errado**: `https://sua-url-railway.app` (sem `/api`)
  
**Como verificar:**
1. Vá em **Settings** → **Environment Variables**
2. Procure por `VITE_API_BASE_URL`
3. Verifique se o valor está no formato: `https://[url-do-railway]/api`

**URL do seu backend Railway:**
- Você precisa copiar a URL pública do Railway (ex: `https://helpful-friendship-production-7f08.up.railway.app`)
- Adicionar `/api` no final
- Formato final: `https://helpful-friendship-production-7f08.up.railway.app/api`

## 🔧 Backend (Railway)

### Configurações Básicas
- [ ] **Root Directory**: `Back` (muito importante!)
- [ ] **Builder**: `Dockerfile` (NÃO Nixpacks!)
- [ ] **Dockerfile Path**: Deixar VAZIO (não digitar nada)
- [ ] **Start Command**: Deixar VAZIO (o Dockerfile já define)

### Variáveis de Ambiente
- [ ] `ASPNETCORE_ENVIRONMENT`: `Production`
- [ ] `ASPNETCORE_URLS`: `http://+:${PORT}`
- [ ] `RAILWAY_VOLUME_MOUNT_PATH`: `/data` (para persistência do SQLite)
- [ ] `FRONTEND_URL`: URL do Vercel (opcional, para CORS)

### Volume Persistente (IMPORTANTE para SQLite)
- [ ] **Volume criado** em Settings → Volumes
- [ ] **Mount Path**: `/data`
- [ ] Variável `RAILWAY_VOLUME_MOUNT_PATH` configurada: `/data`

**Como verificar:**
1. Vá em **Settings** → **Volumes**
2. Deve haver um volume com Mount Path: `/data`
3. Se não houver, clique em **"Add Volume"** e configure

### URL do Backend
- [ ] **URL pública copiada** (ex: `https://helpful-friendship-production-7f08.up.railway.app`)
- [ ] URL está funcionando (teste acessando no navegador)

## 🔗 Integração Frontend ↔ Backend

### Verificações
- [ ] Frontend consegue fazer requisições para o backend
- [ ] CORS está configurado corretamente no backend
- [ ] Teste de login/registro funcionando
- [ ] Teste de compra/devolução funcionando

### Como testar:
1. Acesse a URL do Vercel
2. Abra o Console do navegador (F12)
3. Tente fazer login
4. Verifique se há erros de CORS ou 404/405

## 🐛 Problemas Comuns

### Erro 405 (Method Not Allowed)
- **Causa**: URL do backend incorreta no Vercel
- **Solução**: Verifique se `VITE_API_BASE_URL` termina com `/api`

### Erro CORS
- **Causa**: Backend não está permitindo a origem do Vercel
- **Solução**: O código já permite qualquer origem em produção, mas verifique os logs do Railway

### Dados perdidos após rebuild
- **Causa**: Volume persistente não configurado
- **Solução**: Configure o volume em Settings → Volumes

## 📝 Próximos Passos

1. **Verifique cada item do checklist acima**
2. **Teste a aplicação** no Vercel
3. **Verifique os logs** se houver erros:
   - Vercel: Deployments → Clique no deploy → Logs
   - Railway: Deployments → Clique no deploy → Logs

