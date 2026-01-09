import { jest } from '@jest/globals';

jest.unstable_mockModule('../../../../content-management/helpers/delete-entry.js', () => ({
  default: jest.fn().mockImplementation(() => Promise.resolve()),
}));

const mockGetMany = jest.fn();
const clientMock = {
  entry: {
    getMany: mockGetMany,
  },
};

const getAndValidateClientMock = jest.fn().mockImplementation(() => Promise.resolve(clientMock));

jest.unstable_mockModule('../../../../content-management/helpers/get-client.js', () => ({
  default: getAndValidateClientMock,
}));

const getClient = (await import('../../../../content-management/helpers/get-client.js')).default;

const deleteEntry = (await import('../../../../content-management/helpers/delete-entry.js'))
  .default;

const deleteUnlinkedHeaders = (
  await import('../../../../content-management/changes/20240916-1200-delete-unlinked-headers.js')
).default;

describe('deleteUnlinkedHeaders', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should delete headers with no links', async () => {
    const mockClient = {
      entry: {
        getMany: jest
          .fn()
          .mockImplementationOnce(() =>
            Promise.resolve({
              items: [{ sys: { id: 'header1' } }],
            }),
          )
          .mockImplementationOnce(() =>
            Promise.resolve({
              items: [],
            }),
          ),
      },
    };

    getClient.mockImplementation(() => Promise.resolve(mockClient));

    await deleteUnlinkedHeaders();

    expect(deleteEntry).toHaveBeenCalledWith({ sys: { id: 'header1' } });
  });

  it('should not delete headers with links', async () => {
    const mockClient = {
      entry: {
        getMany: jest
          .fn()
          .mockImplementationOnce(() =>
            Promise.resolve({
              items: [{ sys: { id: 'header2' } }],
            }),
          )
          .mockImplementationOnce(() =>
            Promise.resolve({
              items: [{ sys: { id: 'linked1' } }],
            }),
          ),
      },
    };

    getClient.mockImplementation(() => Promise.resolve(mockClient));

    await deleteUnlinkedHeaders();

    expect(deleteEntry).not.toHaveBeenCalled();
  });

  it('should handle multiple headers correctly', async () => {
    const mockClient = {
      entry: {
        getMany: jest
          .fn()
          .mockImplementationOnce(() =>
            Promise.resolve({
              items: [{ sys: { id: 'header1' } }, { sys: { id: 'header2' } }],
            }),
          )
          .mockImplementationOnce(() =>
            Promise.resolve({
              items: [],
            }),
          )
          .mockImplementationOnce(() =>
            Promise.resolve({
              items: [{ sys: { id: 'linked1' } }],
            }),
          ),
      },
    };

    getClient.mockImplementation(() => Promise.resolve(mockClient));

    await deleteUnlinkedHeaders();

    expect(deleteEntry).toHaveBeenCalledTimes(1);
    expect(deleteEntry).toHaveBeenCalledWith({ sys: { id: 'header1' } });
  });
});
