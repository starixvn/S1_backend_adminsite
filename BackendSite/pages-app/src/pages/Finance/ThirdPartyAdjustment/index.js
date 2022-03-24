import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import { api } from '../../../module/WebAPI';
import qs from 'qs';
import { RadioBtn } from './component/RadioBox';
import { InputField } from './component/InputBox';
import { CustInfo } from './CustInfo';
import Log from './Log';

const initData = {
    optSite: 0,
    optTp: 0,
    userName: "",
    userTpId: ""
}

const AdjustmentOptionData = {
    1: "Add",
    2: "Minus"
}

const App = () => {
    const [site, setSite] = useState([]);
    const [adjustCategoryList, setAdjustCategoryList] = useState([]);
    const [tp, setTp] = useState([]);
    const [custData, setCustData] = useState();
    const [searchData, setSearchData] = useState({ ...initData });
    const [stateSend, setStateSend] = useState(false);
    const [sendData, setSendData] = useState({});
    const SearchFiled = (datacolumn, v) => {
        setSearchData({ ...searchData, [datacolumn]: v });
    };
    const SendFiled = (datacolumn, v) => {
        setSendData({ ...sendData, [datacolumn]: v });
    };
    const [AdjustmentLog, setAdjustment] = useState({
        Datas: [],
        PageInfo: {
            totalPages: 1,
            pageNumber: 1,
            rowCount: 10
        }
    });
    let siteRadio = "", tpRadio = "";
    useEffect(() => {
        api.post('/api/Common/AdminUserSiteList')
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

    useEffect(() => {
        let mounted = true;
        if (searchData.optSite !== 0) {
            setTp([]);
            setCustData();
            setSearchData({ ...searchData, optTp: 0, userName: "", userTpId: "" });
            setSendData({...sendData, category: 1})
            api.post('/api/Tp/GetTpBySite', qs.stringify({ 'SiteId': searchData.optSite }))
                .then(function (response) {
                    if (mounted) {
                        if (response.data.errorCode === 0) {
                            setTp(response.data === null ? [] : response.data.data);
                        } else {
                            // alert(response.message);
                        }
                    }
                }).catch(function (error) {
                    if (mounted) {
                        alert(error);
                    }
                })
        }
        return () => mounted = false;
    }, [searchData.optSite]);

    useEffect(() => {
        api.post('/apiV2/BaseData/GetReasonList', { FuncKey: 'Finance/PaymentAdjustment' })
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    let list = [];
                    if (response.data.data !== null)
                    {
                        response.data.data.map(x => {
                            list.push({ name: x.description, value: x.reasonId });
                        })
                        setAdjustCategoryList(list);
                    }
                } else {
                    alert(!response.message ? 'Get reason list failed.' : response.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }, []);

    const ClickSearch = () => {
        let mounted = true;
        if (!stateSend) {
            api.post('/api/Tp/GetTpCustInfo', qs.stringify({ 'SiteId': searchData.optSite, 'TpId': searchData.optTp, 'UserName': searchData.userName, 'UserTpId': searchData.userTpId }))
                .then(function (response) {
                    if (mounted) {
                        if (response.data.errorCode === 0) {
                            setCustData(response.data === null ? [] : response.data.data);
                            getLog(1, response.data.data.custId);
                        } else {
                            alert(response.data.message);
                        }
                    }
                }).catch(function (error) {
                    if (mounted) {
                        alert(error);
                    }
                })
        }
        return () => mounted = false;
    }
    const UpdateDate = () => {
        let mounted = true;
        
        if (sendData.adjustmentOpt && sendData.amount) {
            setStateSend(true);
            if (!stateSend) {
                api.post('/api/Tp/SetAdjustmentTpCustInfo',
                    qs.stringify({
                        'SiteId': searchData.optSite,
                        'TpId': searchData.optTp,
                        'UserName': searchData.userName,
                        'UserTpId': searchData.userTpId,
                        'Opt': sendData.adjustmentOpt,
                        'Amount': sendData.amount,
                        'Memo': sendData.memo,
                        'Category': sendData.category
                    }))
                    .then((res) => {
                        if (mounted) {
                            let result = res.data;
                            setStateSend(false);
                            if (result.errorCode == 0) {
                                ClickSearch();
                                setSendData({ adjustmentOpt: "", amount: "", memo: "" });
                            } else {
                                alert(result.message);
                            }
                        }
                    }).catch((error) => {
                        if (mounted) {
                            setStateSend(false);
                            alert(error);
                        }
                    });
            }
        }
        return () => mounted = false;
    }

    const getLog = (PageNumber, custId) => {
        api.post('/api/Tp/GetAdjustmentTpLog',
            qs.stringify({
                SiteId: searchData.optSite,
                TpId: searchData.optTp,
                CustId: custId ? custId : custData.custId,
                PageNumber: PageNumber
            }))
            .then((response) => {
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

    if (site.length === 0) {
        return <></>
    } else {
        siteRadio = site.map((value) => {
            return (
                <RadioBtn key={`site-${value.siteId}`} Id={`site-${value.siteId}`} Value={value.siteId} DisplayText={value.siteName} OnChange={(e) => { SearchFiled('optSite', value.siteId) }} Disabled={stateSend} Checked={searchData.optSite === value.siteId} />
            )
        });
    }
    if (tp.length !== 0) {
        tpRadio = tp.map((value) => {
            return (
                <RadioBtn key={`tp-${value.tpId}`} Id={`tp-${value.tpId}`} Value={value.tpId} DisplayText={value.tpName} OnChange={(e) => { SearchFiled('optTp', value.tpId) }} Disabled={stateSend} Checked={searchData.optTp === value.tpId} />
            )
        });
    }

    return (
        <>
            <div className="container-fluid">
                <div className="filter">
                    <h4 className="page-name">3RD Funds Adjustment</h4>
                    <div className="form-group">
                        <label className="col-form-label d-block">Site Name</label>
                        {siteRadio}
                    </div>
                    {
                        (tpRadio != "") ? <div className="form-group">
                            <label className="col-form-label d-block">Third Party Name</label>
                            {tpRadio}
                        </div> : null
                    }
                    {
                        (searchData.optSite !== 0 && searchData.optTp !== 0) ?
                            <>
                                <div className="form-group">
                                    <InputField
                                        IptType="text"
                                        columnTitle="User Name"
                                        defaultValue=""
                                        OnChange={SearchFiled.bind(null, 'userName')}
                                        OnPressEnter={ClickSearch}
                                        Disabled={stateSend}
                                    />
                                </div>
                                <div className="form-group">
                                    <InputField
                                        IptType="text"
                                        columnTitle="User 3RD Id"
                                        defaultValue=""
                                        OnChange={SearchFiled.bind(null, 'userTpId')}
                                        OnPressEnter={ClickSearch}
                                        Disabled={stateSend}
                                    />
                                </div>
                            </> : null

                    }
                    {
                        (searchData.optSite !== 0 && searchData.optTp !== 0 && (searchData.userName != "" || searchData.userTpId != "")) ?
                            < button className="btn btn-add" style={{ marginTop: '20px', marginBottom: '40px' }} onClick={ClickSearch}>Search</button>
                            : null
                    }
                </div>
                <CustInfo
                    CustData={custData}
                    SendFiled={SendFiled}
                    OnPressEnter={UpdateDate}
                    AdjustmentOptionData={AdjustmentOptionData}
                    StateSend={stateSend}
                    SendData={sendData}
                    AdjustCategory={adjustCategoryList} />
                <hr />
                <Log PageInfo={AdjustmentLog.PageInfo} Datas={AdjustmentLog.Datas} AdjustmentOptionData={AdjustmentOptionData} ChangePage={getLog} />
            </div>
        </>
    )
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}