# 🏦 BankSprint API

Sistema bancário completo com API REST em **ASP.NET Core 10.0** e interface web em **Vanilla JavaScript**. Desenvolvido como projeto acadêmico para fins educacionais.

## 📋 Visão Geral

BankSprint é uma aplicação de banco digital que permite:
- ✅ Registrar e autenticar usuários
- ✅ Gerenciar contas bancárias
- ✅ Realizar depósitos e saques
- ✅ Transferências entre contas
- ✅ Visualizar histórico de transações

**Frontend**: Interface web responsiva integrada no projeto (wwwroot)  
**Backend**: API REST com autenticação JWT e banco de dados MySQL

---

## 🛠️ Tecnologias

### Backend
- **Framework**: ASP.NET Core 10.0
- **Banco de Dados**: MySQL 8.0.36
- **ORM**: Entity Framework Core
- **Autenticação**: JWT (JSON Web Tokens)
- **Logging**: Built-in ILogger

### Frontend
- **HTML5** + **CSS3** + **Vanilla JavaScript**
- **Bootstrap 5.3.3** (CDN)
- **Bootstrap Icons 1.11.3** (CDN)
- **LocalStorage** para tokens JWT

---

## 📦 Requisitos

- **.NET SDK 10.0** ou superior
- **MySQL Server 8.0.36** ou compatível
- **Visual Studio Code** ou **Visual Studio** (recomendado)
- **Git** (opcional)

### Verificar instalações

```bash
# Verificar .NET SDK
dotnet --version

# Verificar MySQL (se instalado)
mysql --version
```

---

## 🚀 Instalação e Configuração

### 1. Clonar/Extrair o Projeto

```bash
cd caminho/para/SprintAPIs_Servicos_Web-main
```

### 2. Instalar .NET 10 SDK

Se não estiver instalado:

1. Acesse: https://dotnet.microsoft.com/download
2. Baixe **.NET 10 SDK** para Windows
3. Execute o instalador
4. Reinicie o terminal/IDE

Verificar instalação:
```bash
dotnet --version
```

### 3. Configurar Banco de Dados

#### Opção A: MySQL Local (Recomendado)

1. **Instalar MySQL** (se necessário)
   - Windows: https://dev.mysql.com/downloads/mysql/
   - Usar `mysql-installer-community`

2. **Criar banco de dados**
   ```bash
   mysql -u root -p < bankSprint/Scripts/criar-banco-mysql.sql
   ```
   - Quando solicitado, digite a senha (padrão: `shepherdcom12`)

3. **Verificar conexão**
   ```bash
   mysql -u root -p -e "SHOW DATABASES;" | grep SistemaBancarioDB
   ```

#### Opção B: Atualizar Connection String

Se o MySQL está em outro servidor/porta, editar `bankSprint/appsettings.json`:

```json
"ConnectionStrings": {
  "ConexaoPadrao": "Server=seu_servidor;Database=SistemaBancarioDB;Uid=seu_usuario;Pwd=sua_senha;SslMode=None;AllowPublicKeyRetrieval=True;TreatTinyAsBoolean=true;"
}
```

### 4. Restaurar Dependências

```bash
cd bankSprint
dotnet restore
```

### 5. Aplicar Migrações (Criar Tabelas)

```bash
dotnet ef database update
```

Se obtiver erro sobre ferramentas EF:
```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

---

## ▶️ Executar o Projeto

```bash
cd bankSprint
dotnet run
```

**Saída esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5121
      Now listening on: https://localhost:7096
```

### Acessar a Aplicação

| Recurso | URL |
|---------|-----|
| **Frontend** | http://localhost:5121 |
| **API** | http://localhost:5121/api |
| **Swagger (Docs)** | http://localhost:5121/swagger |

---

## 📚 Documentação da API

### Autenticação

Todos os endpoints **exceto** `/api/auth/*` requerem autenticação JWT.

**Header obrigatório:**
```
Authorization: Bearer {token_jwt}
```

Token obtido no login válido por **2 horas**.

---

### 🔐 Auth Endpoints

#### Registrar Usuário

```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao@example.com",
  "password": "senha123"
}
```

**Resposta (201 Created):**
```json
{
  "id": 1,
  "name": "João Silva",
  "email": "joao@example.com",
  "balance": 0,
  "role": "Cliente"
}
```

**Erros:**
- `400 Bad Request`: Dados inválidos
- `409 Conflict`: E-mail já cadastrado

---

#### Fazer Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "joao@example.com",
  "password": "senha123"
}
```

**Resposta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-05-13T16:30:00Z",
  "message": "Login realizado com sucesso."
}
```

**Erros:**
- `401 Unauthorized`: E-mail ou senha inválidos

---

### 💰 Transaction Endpoints

#### Fazer Depósito

```http
POST /api/transactions/deposit
Authorization: Bearer {token}
Content-Type: application/json

{
  "amount": 1000.50
}
```

**Resposta (200 OK):**
```json
{
  "id": 1,
  "name": "João Silva",
  "email": "joao@example.com",
  "balance": 1000.50,
  "role": "Cliente"
}
```

---

#### Fazer Saque

```http
POST /api/transactions/withdraw
Authorization: Bearer {token}
Content-Type: application/json

{
  "amount": 100.00
}
```

**Resposta (200 OK):**
```json
{
  "id": 1,
  "name": "João Silva",
  "email": "joao@example.com",
  "balance": 900.50,
  "role": "Cliente"
}
```

**Erros:**
- `400 Bad Request`: Saldo insuficiente ou valor inválido
- `404 Not Found`: Conta não encontrada

---

#### Transferência entre Contas

```http
POST /api/transactions/transfer
Authorization: Bearer {token}
Content-Type: application/json

{
  "toAccountId": 2,
  "amount": 500.00
}
```

**Resposta (200 OK):**
```json
{
  "id": 1,
  "name": "João Silva",
  "email": "joao@example.com",
  "balance": 400.50,
  "role": "Cliente"
}
```

**Erros:**
- `400 Bad Request`: Saldo insuficiente, conta origem == destino, ou valor inválido
- `404 Not Found`: Conta de origem ou destino não encontrada

---

#### Visualizar Extrato

```http
GET /api/transactions
Authorization: Bearer {token}
```

**Resposta (200 OK):**
```json
{
  "account": {
    "id": 1,
    "name": "João Silva",
    "email": "joao@example.com",
    "balance": 400.50,
    "role": "Cliente"
  },
  "transactions": [
    {
      "id": 1,
      "type": 0,
      "amount": 1000.50,
      "date": "2026-05-13T14:00:00Z"
    },
    {
      "id": 2,
      "type": 1,
      "amount": 100.00,
      "date": "2026-05-13T14:05:00Z"
    },
    {
      "id": 3,
      "type": 2,
      "amount": -500.00,
      "date": "2026-05-13T14:10:00Z"
    }
  ]
}
```

**Tipos de Transação:**
- `0` = Depósito
- `1` = Saque
- `2` = Transferência

---

### 👤 Account Endpoints

#### Listar Contas

```http
GET /api/accounts
Authorization: Bearer {token}
```

**Resposta (200 OK):**
```json
[
  {
    "id": 1,
    "name": "João Silva",
    "email": "joao@example.com",
    "balance": 400.50,
    "role": "Cliente"
  },
  {
    "id": 2,
    "name": "Maria Santos",
    "email": "maria@example.com",
    "balance": 2500.00,
    "role": "Cliente"
  }
]
```

**Nota**: Clientes veem apenas sua conta; Admins veem todas.

---

#### Atualizar Perfil

```http
PUT /api/accounts
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "João Silva Atualizado",
  "email": "joao.novo@example.com"
}
```

**Resposta (200 OK):**
```json
{
  "id": 1,
  "name": "João Silva Atualizado",
  "email": "joao.novo@example.com",
  "balance": 400.50,
  "role": "Cliente"
}
```

---

#### Deletar Conta

```http
DELETE /api/accounts
Authorization: Bearer {token}
```

**Resposta (204 No Content)**

---

## 📁 Estrutura do Projeto

```
bankSprint/
├── Controllers/           # Controllers da API
│   ├── AuthController.cs
│   ├── AccountsController.cs
│   └── TransactionsController.cs
├── Models/               # Modelos de dados
│   ├── Account.cs
│   ├── Transaction.cs
│   └── TransactionType.cs
├── DTOs/                 # Data Transfer Objects
│   ├── LoginRequestDto.cs
│   ├── RegisterRequestDto.cs
│   ├── TransferRequestDto.cs
│   └── ...
├── Services/             # Lógica de negócio
│   ├── AuthService.cs
│   ├── AccountService.cs
│   └── TransactionService.cs
├── Repositories/         # Acesso a dados
│   ├── AccountRepository.cs
│   └── TransactionRepository.cs
├── Data/                 # EF Core Context
│   └── AppDbContext.cs
├── wwwroot/              # Frontend estático
│   ├── index.html
│   ├── app.js
│   └── style.css
├── Scripts/              # Scripts SQL
│   └── criar-banco-mysql.sql
├── Program.cs            # Configuração
├── appsettings.json      # Configurações
└── bankSprint.csproj     # Arquivo de projeto
```

---

## ⚙️ Configurações

### appsettings.json

```json
{
  "MySqlServerVersion": "8.0.36-mysql",
  "ConnectionStrings": {
    "ConexaoPadrao": "Server=localhost;Database=SistemaBancarioDB;..."
  },
  "Jwt": {
    "Key": "MinhaSenhaDoBalacobacoSecreta_dessavezvaiviu",
    "Issuer": "SistemaBancario",
    "Audience": "UsuariosSistemaBancario"
  }
}
```

**Para produção**, usar User Secrets ou variáveis de ambiente:
```bash
dotnet user-secrets set "Jwt:Key" "sua-chave-segura-aqui"
```

---

## 🧪 Testar a API

### Via Swagger (Recomendado)

1. Acesse: http://localhost:5121/swagger
2. Expanda cada endpoint
3. Clique em "Try it out"
4. Preencha os parâmetros
5. Clique em "Execute"

### Via cURL

```bash
# Registrar
curl -X POST http://localhost:5121/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "password123"
  }'

# Login
curl -X POST http://localhost:5121/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123"
  }'

# Depósito (substituir TOKEN)
curl -X POST http://localhost:5121/api/transactions/deposit \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"amount": 1000}'
```

### Via Postman/Insomnia

1. Importar coleção (se disponível)
2. Configurar variáveis:
   - `{{base_url}}`: http://localhost:5121
   - `{{token}}`: Copiar token do login
3. Executar requisições

---

## 🐛 Troubleshooting

### Erro: "Connection string 'ConexaoPadrao' não encontrada"
- Verificar `appsettings.json`
- Garantir que MySQL está rodando

### Erro: "Unable to connect to MySQL"
- Verificar se MySQL Server está ativo
- Testar conexão: `mysql -u root -p -e "SELECT 1;"`
- Verificar firewall (porta 3306)

### Erro: "EF Core migrations not found"
- Rodar: `dotnet ef database update`
- Se não funcionar, deletar `AppDbContext.cs` e recriar

### Port 5121 já está em uso
- Mudar em `Properties/launchSettings.json`
- Ou: `dotnet run --urls "http://localhost:5122"`

---

## 📝 Exemplos de Uso Frontend

### JavaScript (Automático)

O frontend está integrado em `wwwroot/` e carrega automaticamente ao acessar http://localhost:5121.

Funcionalidades:
- Login/Registro com validação
- Dashboard com saldo em tempo real
- Notificações fixas no rodapé
- Histórico de transações
- Atalhos para transferências recentes

### Usar API Manualmente

```javascript
// Obter token
const response = await fetch('http://localhost:5121/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'test@example.com',
    password: 'password123'
  })
});

const { token } = await response.json();
localStorage.setItem('banking_jwt', token);

// Fazer depósito com token
const depositResponse = await fetch('http://localhost:5121/api/transactions/deposit', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ amount: 500 })
});

const account = await depositResponse.json();
console.log('Novo saldo:', account.balance);
```

---

## 🔒 Segurança

⚠️ **Este é um projeto acadêmico. Em produção:**

- ✅ Usar HTTPS obrigatório
- ✅ Implementar rate limiting
- ✅ Usar secrets seguros (não em appsettings.json)
- ✅ Validar entrada (XSS, SQL Injection)
- ✅ Implementar logging e auditoria
- ✅ Usar variáveis de ambiente
- ✅ Implementar CORS restritivo
- ✅ Atualizar pacotes regularmente

---

## 📄 Licença

Projeto acadêmico. Livre para uso educacional.

---

## ✍️ Autor

Desenvolvido como projeto de estudo em ASP.NET Core e desenvolvimento full-stack.

---

## 📞 Suporte

Para dúvidas ou problemas:
1. Verificar logs em `bin/Debug/`
2. Consultar Swagger em http://localhost:5121/swagger
3. Verificar banco de dados: `mysql -u root -p SistemaBancarioDB`

---

**Versão**: 1.0.0  
**Data**: Maio 2026  
**Status**: ✅ Completo e funcional
