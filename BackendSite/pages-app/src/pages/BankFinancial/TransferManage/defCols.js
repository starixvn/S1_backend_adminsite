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

export const defTransferType = {
  1: "Deposit",
  2: "Withdrawal",
  3: "Transfer",
  4: "Other",
};

export const initTransferType = [
  { value: 1, displayText: defTransferType[1] },
  { value: 2, displayText: defTransferType[2] },
  { value: 3, displayText: defTransferType[3] },
  { value: 4, displayText: defTransferType[4] },
];

export const initSiteId = [
  { value: 0, displayText: "Soon88" },
  { value: 1, displayText: "Nova88" },
  { value: 2, displayText: "Goal123" },
  { value: 3, displayText: "Bwing" },
];

export const defTransactionType = {
  1: "Only Plus",
  2: "Only Minus",
};

export const initTransactionType = [
  { value: 1, displayText: defTransactionType[1] },
  { value: 2, displayText: defTransactionType[2] },
];
