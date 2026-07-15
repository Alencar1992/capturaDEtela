# Captura Rápida

Protótipo web para capturar uma tela, janela ou guia, gerar uma imagem PNG e copiá-la automaticamente para a área de transferência do Windows.

## Arquitetura

- **Interface:** React + Vite, responsiva e sem backend.
- **Captura:** `navigator.mediaDevices.getDisplayMedia()` abre o seletor seguro do navegador.
- **Imagem:** um quadro do vídeo é desenhado em um `canvas` e convertido em PNG.
- **Área de transferência:** `navigator.clipboard.write()` + `ClipboardItem` copiam o PNG.
- **Hospedagem:** arquivos estáticos no GitHub Pages, publicados pelo GitHub Actions.
- **Modo aplicativo:** manifesto e service worker permitem instalar a experiência e manter a interface disponível offline.

```text
Clique/atalho → seletor do navegador → MediaStream → Canvas → PNG
                                                        ├─ área de transferência
                                                        ├─ prévia local
                                                        └─ download opcional
```

Nenhuma imagem é enviada a servidor. Todo o processamento acontece no navegador.

## Aplicativo nativo para Windows

O repositório também contém uma versão nativa em `desktop/CapturaRapida.Native` para uso em segundo plano:

- fica na bandeja do Windows, sem precisar manter uma janela aberta;
- usa um atalho global configurável, com padrão `Ctrl + Alt + P`;
- captura o monitor inteiro onde o cursor estiver;
- copia a imagem automaticamente para a área de transferência;
- permite salvar opcionalmente uma cópia PNG em uma pasta escolhida;
- gera nomes automáticos com data e hora, como `Captura_2026-07-15_15-42-30-123.png`;
- aceita botões de mouse configurados para enviar o mesmo atalho;
- oferece a opção de iniciar automaticamente com o Windows.

### Obter o executável

O workflow **Build Windows App** gera um único arquivo portátil `CapturaRapida.exe`:

1. Abra a aba **Actions** do repositório.
2. Selecione a execução mais recente de **Build Windows App**.
3. Na seção **Artifacts**, baixe `CapturaRapida-Windows-x64`.
4. Extraia o ZIP e execute `CapturaRapida.exe`.

Na primeira execução, escolha o atalho e clique em **Salvar e ocultar**. Depois, configure o botão desejado no software do mouse para enviar essa combinação.

## Executar localmente

Requisitos: Node.js 22 ou superior.

```bash
npm install
npm run dev
```

Abra a URL indicada pelo Vite no Microsoft Edge ou Google Chrome.

## Testar e gerar a versão de produção

```bash
npm test
npm run build
npm run preview
```

## Publicar no GitHub Pages

1. Crie um repositório vazio no GitHub e envie estes arquivos para a branch `main`.
2. No repositório, abra **Settings → Pages**.
3. Em **Build and deployment**, selecione **GitHub Actions**.
4. O workflow `.github/workflows/deploy-pages.yml` fará os testes, o build e a publicação.

O `base: './'` do Vite permite publicar tanto em `usuario.github.io` quanto em `usuario.github.io/nome-do-repositorio/`.

## Limitações de segurança do navegador

- O navegador sempre exige que a pessoa escolha e autorize uma tela, janela ou guia. Um site não pode capturar silenciosamente a janela ativa.
- A captura e a área de transferência exigem um contexto seguro: HTTPS ou `localhost`. O GitHub Pages já usa HTTPS.
- A cópia automática de PNG funciona melhor no Edge e Chrome para desktop. Se o navegador bloquear, a prévia permanece disponível e o botão **Copiar imagem** permite tentar novamente com um clique.
- O atalho `Alt + Shift + S` funciona enquanto o aplicativo está em foco; páginas web não podem registrar atalhos globais do Windows.
- Uma versão nativa com atalho global e captura silenciosa exigiria outra arquitetura, como Tauri ou Electron, e instalação no Windows.

## Estrutura

```text
src/
  components/       componentes visuais e ícones
  lib/capture.js    captura, PNG, clipboard e download
  App.jsx           estados e fluxo principal
public/              manifesto, ícone e service worker
.github/workflows/   publicação automática no GitHub Pages
```
