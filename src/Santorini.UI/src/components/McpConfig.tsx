import { useState } from 'react'

type ClientTab = 'claude' | 'vscode'

const CLIENT_LABELS: Record<ClientTab, string> = {
  claude: 'Claude Desktop / claude.ai',
  vscode: 'VS Code',
}

function buildConfig(client: ClientTab, mcpUrl: string): string {
  const configs: Record<ClientTab, object> = {
    claude: {
      mcpServers: {
        santorini: { url: mcpUrl },
      },
    },
    vscode: {
      servers: {
        santorini: { url: mcpUrl },
      },
    },
  }
  return JSON.stringify(configs[client], null, 2)
}

export default function McpConfig() {
  const [activeTab, setActiveTab] = useState<ClientTab>('claude')
  const [copied, setCopied] = useState(false)

  const mcpUrl = `${window.location.origin}/mcp`
  const configJson = buildConfig(activeTab, mcpUrl)

  const handleCopy = async () => {
    await navigator.clipboard.writeText(configJson)
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <div className="mcp-config">
      <h2 className="mcp-config-title">Connect via MCP</h2>
      <p className="mcp-config-subtitle">
        Add this to your MCP client configuration to play Santorini with an AI agent.
      </p>

      <div className="mcp-config-tabs">
        {(Object.keys(CLIENT_LABELS) as ClientTab[]).map((tab) => (
          <button
            key={tab}
            className={`mcp-tab-btn${activeTab === tab ? ' active' : ''}`}
            onClick={() => setActiveTab(tab)}
          >
            {CLIENT_LABELS[tab]}
          </button>
        ))}
      </div>

      <div className="mcp-config-code-wrapper">
        <button className="mcp-copy-btn" onClick={handleCopy}>
          {copied ? '✓ Copied!' : 'Copy'}
        </button>
        <pre className="mcp-config-code">{configJson}</pre>
      </div>

      <p className="mcp-config-url">
        MCP endpoint: <code>{mcpUrl}</code>
      </p>
    </div>
  )
}
