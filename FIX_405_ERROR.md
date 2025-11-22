# 🔧 Como Corrigir Erro 405 (Method Not Allowed)

## Problema
Erro ao fazer login/registro: `POST https://test-fort-ulwx.vercel.app/helpful-friendship-production-7f08.up.railway.app/auth/register 405 (Method Not Allowed)`

## Causa
A variável `VITE_API_BASE_URL` no Vercel está configurada incorretamente. A URL está sendo tratada como caminho relativo em vez de URL absoluta.

## ✅ Solução Rápida

### 1. Acesse o Vercel
1. Vá em: https://vercel.com/marcelleaps-projects/test-fort-ulwx
2. Clique em **Settings** → **Environment Variables**

### 2. Verifique/Corrija a Variável
1. Procure por `VITE_API_BASE_URL`
2. **Valor CORRETO deve ser**:
   ```
   https://helpful-friendship-production-7f08.up.railway.app/api
   ```
3. **Se estiver diferente**, edite e corrija:
   - ❌ **ERRADO**: `helpful-friendship-production-7f08.up.railway.app`
   - ❌ **ERRADO**: `helpful-friendship-production-7f08.up.railway.app/api` (sem https://)
   - ❌ **ERRADO**: `https://helpful-friendship-production-7f08.up.railway.app` (sem /api)
   - ✅ **CORRETO**: `https://helpful-friendship-production-7f08.up.railway.app/api`

### 3. Faça Redeploy
1. Vá em **Deployments**
2. Clique nos três pontos do último deploy
3. Selecione **"Redeploy"**
4. Aguarde o deploy completar

### 4. Teste Novamente
1. Acesse a URL do Vercel
2. Tente fazer login/registro
3. Deve funcionar agora!

## 💡 O que foi corrigido no código

O código agora **normaliza automaticamente** a URL, então mesmo se você configurar:
- `helpful-friendship-production-7f08.up.railway.app` → será corrigido para `https://helpful-friendship-production-7f08.up.railway.app/api`
- `helpful-friendship-production-7f08.up.railway.app/api` → será corrigido para `https://helpful-friendship-production-7f08.up.railway.app/api`

Mas é melhor configurar corretamente desde o início!

## 📝 Checklist

- [ ] Variável `VITE_API_BASE_URL` configurada no Vercel
- [ ] Valor começa com `https://`
- [ ] Valor termina com `/api`
- [ ] Formato completo: `https://helpful-friendship-production-7f08.up.railway.app/api`
- [ ] Redeploy feito após corrigir
- [ ] Teste de login/registro funcionando

