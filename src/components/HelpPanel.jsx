import { CaptureIcon, CopyIcon, LockIcon } from './Icons.jsx'

const steps = [
  {
    icon: <CaptureIcon />,
    title: 'Clique em Capturar tela',
    description: 'Inicie a captura da sua tela.',
  },
  {
    icon: (
      <svg aria-hidden="true" width="24" height="24" viewBox="0 0 24 24" fill="none">
        <rect x="3" y="4" width="18" height="16" rx="2.5" stroke="currentColor" strokeWidth="1.8" />
        <path d="M3 9h18" stroke="currentColor" strokeWidth="1.8" />
      </svg>
    ),
    title: 'Escolha o conteúdo',
    description: 'Selecione uma tela, janela ou guia do navegador.',
  },
  {
    icon: <CopyIcon />,
    title: 'Cole com Ctrl + V',
    description: 'Use a captura em uma conversa, documento ou editor.',
  },
]

export default function HelpPanel() {
  return (
    <aside className="help-panel" id="ajuda" aria-labelledby="help-title">
      <h2 id="help-title">Como funciona</h2>
      <ol className="steps">
        {steps.map((step, index) => (
          <li className="step" key={step.title}>
            <span className="step-number">{index + 1}</span>
            <span className="step-icon">{step.icon}</span>
            <span className="step-copy">
              <strong>{step.title}</strong>
              <span>{step.description}</span>
            </span>
          </li>
        ))}
      </ol>
      <div className="privacy-note">
        <LockIcon />
        <p>A imagem não sai do seu dispositivo.</p>
      </div>
    </aside>
  )
}
