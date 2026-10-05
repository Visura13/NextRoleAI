import { beforeEach, describe, expect, it, vi } from 'vitest';
import { api } from './client';

describe('ApiClient', () => {
  beforeEach(() => {
    api.clearSession();
    vi.restoreAllMocks();
  });

  it('lets the browser set the multipart boundary for FormData', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ accepted: true }), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      }),
    );
    const body = new FormData();
    body.append('file', new File(['cv'], 'cv.pdf', { type: 'application/pdf' }));

    await api.request('/api/cv', { method: 'POST', body });

    const init = fetchMock.mock.calls[0][1] as RequestInit;
    expect(new Headers(init.headers).has('Content-Type')).toBe(false);
  });
});
