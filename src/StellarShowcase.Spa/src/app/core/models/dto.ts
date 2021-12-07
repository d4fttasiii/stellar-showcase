export interface AccountDto {
    accountId: string;
    sequenceNumber: number;
    nativeBalance: number;
    nonNativeBalances: Balance[];
    isAuthRequired: boolean;
    isAuthRevocable: boolean;
    isAuthImmutable: boolean;
    isClawbackEnabled: boolean;
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

export interface UpsertAssetDto {
    unitName: string;
    totalSupply: number;
    isAuthRequired: boolean;
    isAuthRevocable: boolean;
    isAuthImmutable: boolean;
    isClawbackEnabled: boolean;
}

export interface AssetDto {
    id: string;
    issuerId: string;
    unitName: string;
    totalSupply: number;
    issuerAccountId: string;
}

export interface IssuerDto {
    id: string;
    issuerAccountId: string;
    distributorAccountId: string;
    issuer: AccountDto;
    distributor: AccountDto;
    assets: AssetDto[];
}

export interface IssuerTransferDto {
    userAccountId: string;
    amount: number;
    memo: string;
}

export interface OrderBookDto {
    buys: Buy[];
    sells: Sell[];
}

export interface Buy {
    volume: number;
    price: number;
}

export interface Sell extends Buy {}

export interface CreateMarketDto {
    name: string;
    baseAssetId: string;
    quoteAssetId: string;
}

export interface MarketDto {
    id: string;
    name: string;
    base: AssetDto;
    quote: AssetDto;
    orderBooks: OrderBookDto;
}