import Keycloak from 'keycloak-js'

const keycloak = new Keycloak({
  url: import.meta.env.VITE_KEYCLOAK_URL,
  realm: import.meta.env.VITE_KEYCLOAK_REALM,
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
})

export function initKeycloak(onSuccess: (authenticated: boolean) => void): void {
  keycloak
    .init({ onLoad: 'login-required', pkceMethod: 'S256' })
    .then(onSuccess)
    .catch((err: unknown) => {
      console.error('Keycloak init failed', err)
      onSuccess(false)
    })
}

export default keycloak
