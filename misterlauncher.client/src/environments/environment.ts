declare global {
  interface Window {
    env: any;
  }
}
export const environment = {
  production: true,
  urlGameApi:
    window['env']['backapiurl'] || 'default'
};
