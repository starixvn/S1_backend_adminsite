import React, { useState, useEffect } from 'react';
import ReactDOM from 'react-dom';
import Filter from './Filter';
import Result from './Result';
import { FilterColumns } from './defColumns';
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';
import Log from './AdjustmentLog';
import Setting from './Setting';

const Panel = () => {
    const [site, setSite] = useState([]);
    const [custInfo, setCustInfo] = useState();
    const [loading, setLoading] = useState();
    const [AdjustmentLog, setAdjustment] = useState({
        Datas: [],
        PageInfo: {
            totalPages: 1,
            pageNumber: 1,
            rowCount: 10
        }
    });

    const onSearch = async (datas) => {
        await GetCustInfo(datas.optSite, datas[FilterColumns.userName.dataColumns], datas[FilterColumns.sabaId.dataColumns]);
    }

    useEffect(() => {
        let param = {
            Url: 'Common/AdminUserSiteList'
        };
        WebAPI.PostData(param)
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    setSite(response.data === null ? [] : response.data.data);
                } else {
                    // alert(response.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }, []);

    const GetCustInfo = (siteId, userName, sabaId) => {
        let param = {
            Url: 'AdjustWithdrawalLimit/GetCustAndWalletInfo',
            Data: qs.stringify({
                SiteId: siteId,
                UserName: userName,
                SabaId: sabaId
            })
        };
        setCustInfo(null);
        setLoading(true);
        WebAPI.PostData(param).then((res) => {
            setLoading(false);
            let result = res.data;
            if (result.errorCode == 0) {
                setCustInfo(result.data);
                QueryWithdrawalLimitLog(1, result.data.custId);
            } else {
                alert(result.message);
            }
        }).catch((error) => {
            setLoading(false);
            alert(error);
        });
    }

    const onSetWithdrawalLimit = (datas) => {
        let param = {
            Url: 'AdjustWithdrawalLimit/SetAdjustWithdrawalLimit',
            Data: qs.stringify({
                CustId: custInfo.custId,
                adjustWithdrawalLimit: {
                    AdjustType: datas.actionType,
                    Amount: datas.amount,
                    Memo: datas.note
                }
            })
        };

        WebAPI.PostData(param).then((res) => {
            let result = res.data;
            if (result.errorCode == 0) {
                GetWalletInfo(custInfo.custId);
                QueryWithdrawalLimitLog(1, custInfo.custId);
            } else {
                alert(result.message);
            }
        }).catch((error) => {
            alert(error);
        });
    }

    const QueryWithdrawalLimitLog = (PageNumber, custId) => {
        let param = {
            Url: 'AdjustWithdrawalLimit/GetAdjustWithdrawalLimitsLog',
            Data: qs.stringify({
                CustId: custId ? custId : custInfo.custId, PageNumber: PageNumber
            })
        };

        WebAPI.PostData(param).then((response) => {
            let result = response.data;
            if (result.errorCode == 0) {
                setAdjustment({
                    Datas: result.data,
                    PageInfo: {
                        totalPages: result.totalPages,
                        pageNumber: PageNumber,
                        rowCount: result.rowCount
                    }
                });
            } else {
                alert(result.message);
            }
        }).catch((error) => {
            alert(error);
        });
    }

    const GetWalletInfo = (custId) => {
        let param = {
            Url: 'AdjustWithdrawalLimit/GetWalletInfo',
            Data: qs.stringify({
                CustId: custId
            })
        };

        WebAPI.PostData(param).then((response) => {
            let result = response.data;
            if (result.errorCode == 0) {
                setCustInfo((custInfo) => setCustInfo({
                    ...custInfo,
                    balance: result.data.balance,
                    differenceAmount: result.data.differenceAmount,
                    eligibleWithdrawalAmount: result.data.availableBalance
                }));
            } else {
                alert(result.message);
            }
        }).catch((error) => {
            alert(error);
        });
    }

    return (
        <div className="container-fluid">
            <h4 className="page-name">Withdrawal Limit Adjustment</h4>
            <Filter Site={site} ClickSubmit={onSearch} />
            {
                loading ? 'Loading...' : custInfo === undefined || custInfo === null ? <table style={{ height: '100%' }}></table> : (
                    <>
                        <Result CustInfo={custInfo} onSetWithdrawalLimit={onSetWithdrawalLimit} />
                        <Setting onSetWithdrawalLimit={onSetWithdrawalLimit} differenceAmount={custInfo.differenceAmount} />
                        <hr />
                        <Log PageInfo={AdjustmentLog.PageInfo} Datas={AdjustmentLog.Datas} ChangePage={QueryWithdrawalLimitLog} />
                    </>
                )
            }
        </div>
    )
}

export default Panel;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<Panel />, document.getElementById('content'))
}