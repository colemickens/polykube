import { PolykubeFrontendPage } from './app.po';

describe('polykube-frontend App', function() {
  let page: PolykubeFrontendPage;

  beforeEach(() => {
    page = new PolykubeFrontendPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
