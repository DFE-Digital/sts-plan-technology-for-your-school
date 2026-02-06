function ShouldBeAuthorised(page) {
  cy.request({
    url: page.fields.slug,
    followRedirect: false,
  }).then((resp) => {
    expect(resp.status).to.eq(302);
    expect(resp.redirectedToUrl).to.contain('https://pp-oidc.signin.education.gov.uk/');
  });
}

export default ShouldBeAuthorised;
