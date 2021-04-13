export const defChannelType = {
  1: "Local Bank",
  2: "Head Office",
  3: "PSP",
};

export const initChannelType = [
  { value: 0, displayText: "- Select Channel -" },
  { value: 1, displayText: defChannelType[1] },
  { value: 2, displayText: defChannelType[2] },
  { value: 3, displayText: defChannelType[3] },
];

export const defStatusType = {
  1: "Initial",
  2: "Open",
  3: "Suspend",
  4: "Close",
};

export const initStatusType = [
  { value: 1, displayText: defStatusType[1] },
  { value: 2, displayText: defStatusType[2] },
  { value: 3, displayText: defStatusType[3] },
  { value: 4, displayText: defStatusType[4] },
];

export const defPurposeType = {
  1: "Deposit",
  2: "Withdrawal",
  3: "Transfer",
};

export const initPurposeType = [
  { value: 1, displayText: defPurposeType[1] },
  { value: 2, displayText: defPurposeType[2] },
  { value: 3, displayText: defPurposeType[3] },
];

export const initSiteId = [
  { value: 1, displayText: "Nova88" },
  { value: 2, displayText: "Goal123" },
  { value: 3, displayText: "BWing" },
];
