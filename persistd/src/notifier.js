const { spawn } = require('node:child_process');

function display(value, fallback = 'NONE') {
  return value == null || value === '' ? fallback : value;
}

function buildTerminalMessage(state) {
  const generation = display(state.GENERATION, '?');
  return [
    `/persist concluído — G${generation}`,
    `RUN_ID: ${display(state.RUN_ID)}`,
    `Projeto: ${display(state.PROJECT_ROOT)}`,
    `HEAD final: ${display(state.HEAD)}`,
    'DoD verificado pelo worker terminal e estado durável marcado como DONE.',
  ].join('\n');
}

function buildAttentionMessage(state, kind) {
  return [
    `/persist precisa de você — ${kind}`,
    `RUN_ID: ${display(state.RUN_ID)}`,
    `Geração ativa: G${display(state.GENERATION, '?')}`,
    `Próxima ação: ${display(state.NEXT_SAFE_ACTION)}`,
  ].join('\n');
}

function showWindowsToast(title, body) {
  if (process.platform !== 'win32') return Promise.resolve({ sent: false, channel: 'windows-toast', reason: 'not-windows' });
  const script = `
[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null
[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] > $null
$title = [System.Security.SecurityElement]::Escape($env:PERSISTD_TOAST_TITLE)
$body = [System.Security.SecurityElement]::Escape($env:PERSISTD_TOAST_BODY)
$xml = New-Object Windows.Data.Xml.Dom.XmlDocument
$xml.LoadXml("<toast><visual><binding template='ToastGeneric'><text>$title</text><text>$body</text></binding></visual></toast>")
$toast = [Windows.UI.Notifications.ToastNotification]::new($xml)
[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('persistd').Show($toast)
`;
  return new Promise((resolve) => {
    const child = spawn('powershell.exe', ['-NoProfile', '-NonInteractive', '-Command', script], {
      windowsHide: true,
      env: { ...process.env, PERSISTD_TOAST_TITLE: title, PERSISTD_TOAST_BODY: body },
      stdio: 'ignore',
    });
    child.on('error', () => resolve({ sent: false, channel: 'windows-toast' }));
    child.on('close', (code) => resolve({ sent: code === 0, channel: 'windows-toast' }));
  });
}

function createNotifier({ browser, localToast = showWindowsToast } = {}) {
  return {
    async terminal({ state, message }) {
      if (browser?.sendTerminalNotification) {
        try {
          const result = await browser.sendTerminalNotification({ state, message });
          if (result?.sent) return { sent: true, channel: 'chatgpt' };
        } catch {}
      }
      return localToast('/persist concluído', message);
    },
    async attention({ state, kind, message }) {
      return localToast(`/persist — ${kind}`, message);
    },
  };
}

module.exports = { buildTerminalMessage, buildAttentionMessage, showWindowsToast, createNotifier };
