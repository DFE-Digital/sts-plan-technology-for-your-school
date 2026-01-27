import { jest } from '@jest/globals';

type ContentfulSpace = {
  getWebhooks: () => Promise<any>;
  createWebhook: () => Promise<any>;
};
type ContentfulManagementClient = {
  getSpace: () => Promise<ContentfulSpace>;
};
// Mock modules at the top level
const mockCreateClient = jest.fn();
const mockGetOptions = jest.fn();
const mockSpace: any = {
  getWebhooks: jest.fn(),
  createWebhook: jest.fn(),
};

jest.mock(
  'contentful-management',
  () => ({
    createClient: mockCreateClient,
  }),
  { virtual: true },
);

jest.mock('../../../webhook-creator/src/options', () => ({
  getOptions: mockGetOptions,
}));

describe('createOrUpdateWebhook', () => {
  // Mock data
  const mockOptions = {
    MANAGEMENT_TOKEN: 'mock-token',
    SPACE_ID: 'mock-space',
    WEBHOOK_API_KEY: 'mock-api-key',
    WEBHOOK_NAME: 'mock-name',
    ENVIRONMENT_NAME: 'mock-env',
    ENVIRONMENT_ID: 'mock-env-id',
    WEBHOOK_URL: 'http://mock-url.com',
  };

  const mockWebhook: any = {
    sys: { id: 'mock-webhook-id' },
    url: 'http://mock-url.com',
    update: jest.fn(),
    headers: [],
    name: '',
    filters: [],
    topics: [],
  };

  let createOrUpdateWebhook;

  beforeEach(async () => {
    jest.clearAllMocks();

    // Configure the mock implementation for this test
    mockCreateClient.mockImplementation(() => ({
      getSpace: jest.fn().mockImplementation(() => Promise.resolve(mockSpace)),
    }));

    mockSpace.getWebhooks.mockImplementation(() => Promise.resolve({ items: [] }));

    mockGetOptions.mockImplementation(() => ({
      result: mockOptions,
      success: true,
      error: undefined,
    }));
    try {
      const functions = await import('../../../webhook-creator/src/contentful-webhook-functions');

      createOrUpdateWebhook = functions.createOrUpdateWebhook;
    } catch (e) {
      console.error(e);
      throw e;
    }
  });

  describe('upserts', () => {
    it('should create new webhook when none exists', async () => {
      mockSpace.getWebhooks.mockImplementation(() => Promise.resolve({ items: [] }));

      await createOrUpdateWebhook();

      expect(mockSpace.createWebhook).toHaveBeenCalledTimes(1);
      expect(mockSpace.createWebhook).toHaveBeenCalledWith(
        expect.objectContaining({
          name: `${mockOptions.WEBHOOK_NAME} - ${mockOptions.ENVIRONMENT_NAME}`,
          url: mockOptions.WEBHOOK_URL,
        }),
      );
    });

    it('should update existing webhook when URL matches', async () => {
      // Arrange
      mockSpace.getWebhooks.mockImplementationOnce(() =>
        Promise.resolve({
          items: [mockWebhook],
        }),
      );

      // Act
      await createOrUpdateWebhook();

      // Assert
      expect(mockSpace.createWebhook).not.toHaveBeenCalled();
      expect(mockWebhook.update).toHaveBeenCalledTimes(1);
    });
  });

  describe('handles errors', () => {
    it('should handle error when creating space client', async () => {
      const errorMessage = 'Space client error';
      (mockCreateClient as jest.Mock).mockImplementationOnce(() => ({
        getSpace: jest.fn().mockImplementationOnce(() => Promise.reject(Error(errorMessage))),
      }));

      await expect(createOrUpdateWebhook).rejects.toThrow(errorMessage);

      expect(mockSpace.getWebhooks).not.toHaveBeenCalled();
    });

    it('should handle error when creating webhook', async () => {
      mockSpace.createWebhook.mockImplementationOnce(() =>
        Promise.reject(new Error('Webhook creation error')),
      );

      await expect(createOrUpdateWebhook).rejects.toThrow();

      expect(mockSpace.createWebhook).toHaveBeenCalledTimes(1);
    });

    it('should handle error when options are invalid', async () => {
      (mockGetOptions as jest.Mock).mockImplementation(() => ({
        success: false,
        result: null,
        error: 'Invalid options',
      }));

      await expect(createOrUpdateWebhook).rejects.toThrow();

      expect(mockCreateClient).not.toHaveBeenCalled();
    });

    it('should return early if space client is falsy', async () => {
      (mockCreateClient as jest.Mock).mockImplementationOnce(() => ({
        getSpace: jest.fn().mockImplementationOnce(() => Promise.resolve(null)),
      }));

      await createOrUpdateWebhook();

      expect(mockSpace.getWebhooks).not.toHaveBeenCalled();
      expect(mockSpace.createWebhook).not.toHaveBeenCalled();
    });

    describe('handles update errors', () => {
      it('should not update if errors on retrieval', async () => {
        mockSpace.getWebhooks.mockImplementationOnce(() =>
          Promise.reject(Error('Error getting webhooks')),
        );

        (mockWebhook.update as jest.Mock).mockImplementationOnce(() =>
          Promise.reject(new Error('error')),
        );

        await expect(createOrUpdateWebhook).rejects.toThrow();

        expect(mockWebhook.update).toHaveBeenCalledTimes(0);
      });

      it('should handle errors on update', async () => {
        mockSpace.getWebhooks.mockImplementationOnce(() =>
          Promise.resolve({
            items: [mockWebhook],
          }),
        );

        (mockWebhook.update as jest.Mock).mockImplementationOnce(() =>
          Promise.reject(new Error('error')),
        );

        await expect(createOrUpdateWebhook).rejects.toThrow();

        expect(mockWebhook.update).toHaveBeenCalledTimes(1);
      });
    });
  });
});
