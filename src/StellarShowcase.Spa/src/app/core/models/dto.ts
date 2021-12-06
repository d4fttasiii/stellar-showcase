export interface AccountDto {
    accountId: string;
    sequenceNumber: number;
    nativeBalance: number;
    nonNativeBalances: Balance[];
}

export interface Balance {
    assetType: string;
    assetCode: string;
    assetIssuer: string;
    amount: number;
}

export interface CreateUserAccountDto {
    fullName: string;
    fullAddress: string;
    email: string;
    phone: string;
}

export interface UserAccountDto extends CreateUserAccountDto {
    id: string;
    account: AccountDto;
}