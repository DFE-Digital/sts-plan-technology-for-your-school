export const MAT_SCHOOLS = {
  COMMUNITY: {
    URN: '900006',
    NAME: 'DSI TEST Establishment (001) Community School (01)',
  },
  MISCELLANEOUS: {
    URN: '900008',
    NAME: 'DSI TEST Establishment (001) Miscellaneous (27)',
  },
  FOUNDATION: {
    URN: '900007',
    NAME: 'DSI TEST Establishment (001) Foundation School (05)',
  },
} as const;

export const ALL_MAT_SCHOOLS = Object.values(MAT_SCHOOLS);

export const MAT_SCHOOLS_BY_URN: Record<string, { URN: string; NAME: string }> = Object.fromEntries(
  Object.values(MAT_SCHOOLS).map((school) => [school.URN, school]),
);

export const MAT_SCHOOLS_BY_NAME: Record<string, { URN: string; NAME: string }> =
  Object.fromEntries(Object.values(MAT_SCHOOLS).map((school) => [school.NAME, school]));
