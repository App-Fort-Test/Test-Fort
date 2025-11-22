# 🔧 Como Corrigir Erro "Railpack could not determine how to build the app"

## Problema
O Railway está tentando usar Railpack/Nixpacks em vez do Dockerfile, resultando no erro:
```
⚠ Script start.sh not found
✖ Railpack could not determine how to build the app.
```

## ✅ Solução URGENTE

### Passo 1: Acesse as Configurações do Railway
1. Acesse: https://railway.com/project/f91c4260-84da-457e-9311-5da58bedc6f9/service/395283a7-0e23-492b-a4e3-a02aebb6fb76/settings
2. Vá em **Settings** → **Build**

### Passo 2: Configure o Builder para Dockerfile
1. Procure a seção **"Builder"** ou **"Build Method"**
2. **MUDE de "Nixpacks" ou "Auto-detect" para "Dockerfile"**
   - ⚠️ **CRÍTICO**: Deve estar como **"Dockerfile"**, não "Nixpacks" ou "Railpack"
3. Em **"Dockerfile Path"**, **DEIXE VAZIO** (não digite nada)
   - Com Root Directory = `Back`, o Railway encontra o Dockerfile automaticamente
4. **Salve as configurações**

### Passo 3: Verifique o Root Directory
1. Vá em **Settings** → **Source**
2. Verifique se **Root Directory** está configurado como: `Back`
3. Se não estiver, configure agora

### Passo 4: Limpe o Cache
1. Vá em **Settings** → **Deploy**
2. Procure por **"Clear Build Cache"** ou **"Clear Cache"**
3. Clique para limpar o cache

### Passo 5: Force um Novo Deploy
1. Vá em **Deployments**
2. Clique nos três pontos do último deploy
3. Selecione **"Redeploy"**
4. Aguarde o build completar

## 📋 Checklist

- [ ] Builder configurado como **"Dockerfile"** (não Nixpacks)
- [ ] Dockerfile Path está **VAZIO**
- [ ] Root Directory configurado como **`Back`**
- [ ] Cache limpo
- [ ] Redeploy feito

## 🔍 Como Verificar se Está Correto

Após o redeploy, os logs devem mostrar:
- `Building Docker image...` ou similar
- **NÃO deve aparecer**: "Railpack" ou "Nixpacks"

Se ainda aparecer erro, verifique:
1. O Dockerfile existe em `Back/Dockerfile`?
2. O Dockerfile foi commitado no Git?
3. O Root Directory está como `Back`?

## ⚠️ Importante

O Railway às vezes "esquece" a configuração do Builder. Sempre verifique após:
- Criar um novo serviço
- Fazer push de código
- Atualizar configurações

Se o problema persistir, delete o serviço e crie novamente, configurando o Builder ANTES do primeiro deploy.

