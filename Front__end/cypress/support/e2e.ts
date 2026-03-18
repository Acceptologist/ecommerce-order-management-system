import './commands';

Cypress.on('uncaught:exception', (err, runnable) => {
  // prevent failing for app side uncaught exceptions during e2e runs
  return false;
});