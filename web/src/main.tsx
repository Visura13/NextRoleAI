import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router';
import { SWRConfig } from 'swr';
import { Application } from './Application';
import { AuthProvider } from './auth/AuthContext';
import './styles.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <SWRConfig value={{ revalidateOnFocus: false, shouldRetryOnError: false }}>
        <AuthProvider>
          <Application />
        </AuthProvider>
      </SWRConfig>
    </BrowserRouter>
  </StrictMode>,
);
