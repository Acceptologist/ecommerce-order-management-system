// custom commands for reusable steps
Cypress.Commands.add('login', (username: string, password: string) => {
  cy.request('POST', `${Cypress.env('apiUrl') || 'http://localhost:5000'}/api/auth/login`, { username, password })
    .then((response) => {
      window.localStorage.setItem('access_token', response.body.accessToken);
      window.localStorage.setItem('refresh_token', response.body.refreshToken);
      return response.body;
    });
});
