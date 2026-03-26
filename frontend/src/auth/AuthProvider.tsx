import React, { createContext, useContext, useEffect, useRef, useState } from 'react'
import keycloak, { initKeycloak } from './keycloak'
import { setAuthToken } from '../api/client'
import { registerUser } from '../api/users'

interface AuthState {
  authenticated: boolean
  token: string
  userRoles: string[]
  userId: string
  internalUserId: string
}

const AuthContext = createContext<AuthState>({
  authenticated: false,
  token: '',
  userRoles: [],
  userId: '',
  internalUserId: '',
})

export function useAuthStore(): AuthState {
  return useContext(AuthContext)
}

interface AuthProviderProps {
  children: React.ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [initialized, setInitialized] = useState(false)
  const [authState, setAuthState] = useState<AuthState>({
    authenticated: false,
    token: '',
    userRoles: [],
    userId: '',
    internalUserId: '',
  })
  const refreshIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null)

  useEffect(() => {
    initKeycloak((authenticated) => {
      if (authenticated && keycloak.token) {
        const token = keycloak.token
        const roles: string[] = keycloak.realmAccess?.roles ?? []
        const userId = keycloak.subject ?? ''

        setAuthToken(token)
        setAuthState({ authenticated: true, token, userRoles: roles, userId, internalUserId: '' })

        registerUser()
          .then((profile) => {
            setAuthState((prev) => ({ ...prev, internalUserId: profile.id }))
          })
          .catch((err: unknown) => {
            console.warn('User registration failed (may already exist)', err)
          })

        refreshIntervalRef.current = setInterval(() => {
          keycloak.updateToken(70).then((refreshed) => {
            if (refreshed && keycloak.token) {
              setAuthToken(keycloak.token)
              setAuthState((prev) => ({ ...prev, token: keycloak.token! }))
            }
          }).catch((err: unknown) => {
            console.error('Token refresh failed', err)
          })
        }, 60_000)
      }

      setInitialized(true)
    })

    return () => {
      if (refreshIntervalRef.current) clearInterval(refreshIntervalRef.current)
    }
  }, [])

  if (!initialized) return null

  return <AuthContext.Provider value={authState}>{children}</AuthContext.Provider>
}
