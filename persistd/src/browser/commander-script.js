function buildCommanderSetupScript() {
  return `
// PERSISTD_COMMANDER_BOOTSTRAP_V1
const commanderAttached = async () => Boolean(await composer.evaluate((el) => {
  const form = el.closest('form')
  const text = String(form && form.innerText || '')
  return Boolean(text.includes('Remote Desktop Commander') || text.includes('Desktop Commander'))
}))
if (!(await commanderAttached())) {
  let attachButton = page.getByTestId('composer-plus-btn')
  if ((await attachButton.count()) === 0) attachButton = page.getByRole('button', { name: /adicionar arquivos e mais|attach files|add files|tools/i })
  if ((await attachButton.count()) === 0) throw new Error('ChatGPT tools button not found')
  await attachButton.first().evaluate((el) => el.click())
  await page.waitForTimeout(250)
  const pluginSearch = page.locator('input[placeholder*="plugins"]')
  if ((await pluginSearch.count()) > 0 && await pluginSearch.first().isVisible()) await pluginSearch.first().fill('Remote Desktop Commander')
  else await page.keyboard.type('Remote Desktop Commander')
  const commander = page.getByText('Remote Desktop Commander', { exact: true })
  let commanderClicked = false
  for (let i = 0; i < await commander.count(); i++) {
    if (!(await commander.nth(i).isVisible())) continue
    await commander.nth(i).evaluate((el) => el.click())
    commanderClicked = true
    break
  }
  if (!commanderClicked) throw new Error('Remote Desktop Commander option not found')
  const verified = await page.waitForFunction(() => {
    const el = document.querySelector('#prompt-textarea') || document.querySelector('[role="textbox"][contenteditable="true"]')
    const form = el && el.closest('form')
    const text = String(form && form.innerText || '')
    return Boolean(text.includes('Remote Desktop Commander') || text.includes('Desktop Commander'))
  }, undefined, { timeout: 5000 })
  if (!verified) throw new Error('Remote Desktop Commander attachment not verified')
}
`;
}

module.exports = { buildCommanderSetupScript };
