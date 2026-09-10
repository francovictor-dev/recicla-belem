# ♻️ Recicla Belém Web

Frontend da plataforma **Recicla Belém**, desenvolvido com **Angular** e **TypeScript**.

O objetivo do projeto é facilitar a conexão entre cidadãos, cooperativas e pontos de coleta, promovendo o descarte correto de materiais recicláveis e incentivando práticas sustentáveis.

## 🚀 Tecnologias

- Angular
- TypeScript
- Angular Router
- RxJS
- Angular HttpClient
- SCSS
- Bootstrap
- JWT Authentication
- ESLint

## 📂 Estrutura do projeto

```
src/
├── public/
├── app/
│   ├── components/
│   ├── layouts/
│   ├── pages/
│   ├── services/
│   ├── store/
│   └── type/
└── environments/
```

> A estrutura pode variar conforme a evolução do projeto.

---

## 📦 Instalação

Clone o repositório:

```bash
git clone https://github.com/francovictor-dev/recicla-belem-web.git
```

Entre na pasta do projeto:

```bash
cd recicla-belem-web
```

Instale as dependências:

```bash
npm install
```

---

## ⚙️ Variáveis de ambiente

Configure os arquivos de ambiente em `src/environments`.

Exemplo:

**environment.ts**

```ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:8000',
};
```

Para produção, utilize o arquivo `environment.prod.ts`.

---

## ▶️ Executando o projeto

Inicie o servidor de desenvolvimento:

```bash
ng serve
```

ou

```bash
npm start
```

A aplicação estará disponível em:

```
http://localhost:4200
```

---

## 📦 Build para produção

```bash
ng build
```

ou

```bash
npm run build
```

Os arquivos compilados serão gerados na pasta:

```
dist/
```

---

## 🧪 Executando testes

Testes unitários:

```bash
ng test
```

Testes end-to-end (caso configurados):

```bash
ng e2e
```

---

## ✨ Funcionalidades

- Autenticação de usuários
- Cadastro e login
- Consulta de pontos de coleta
- Visualização de pontos de coleta
- Cadastro e gerenciamento de materiais recicláveis
- Busca e filtros
- Consumo de API REST
- Interface responsiva

> As funcionalidades podem variar conforme a evolução do projeto.

---

## 📱 Responsividade

A interface foi desenvolvida para proporcionar uma boa experiência em dispositivos móveis, tablets e desktops.

---

## 📁 Projeto relacionado

Este frontend consome a API **Recicla Belém API**, feito em PHP, responsável pela autenticação, gerenciamento de usuários, materiais recicláveis, cooperativas e pontos de coleta.

---

## 📄 Licença

Este projeto foi desenvolvido para fins de estudo e aplicação real.
