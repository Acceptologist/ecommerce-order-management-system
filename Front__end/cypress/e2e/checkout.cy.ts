describe('Checkout payment flow', () => {
  const username = 'admin';
  const password = 'Admin@123';

  before(() => {
    cy.visit('/login');
    cy.get('input[formcontrolname="username"]').type(username);
    cy.get('input[formcontrolname="password"]').type(password);
    cy.get('button[type="submit"]').click();
    cy.url().should('include', '/dashboard');
  });

  it('adds a product to cart and completes checkout with payment simulation', () => {
    cy.visit('/products');

    // Add first available product to cart
    cy.contains('button', 'Add to Cart').first().click();

    // Go to cart and checkout
    cy.visit('/cart');
    cy.contains('button', 'Proceed to Checkout').click();
    cy.url().should('include', '/checkout');

    // Fill address details and place order
    cy.get('input[formcontrolname="street"]').type('123 Cypress Lane');
    cy.get('input[formcontrolname="city"]').type('Testville');
    cy.get('input[formcontrolname="country"]').type('Testland');

    cy.contains('button', /Place Order/).click();

    // Order should be created and redirect to order details
    cy.url().should('match', /\/orders\/\d+$/);
    cy.contains('Order #').should('be.visible');
  });
});