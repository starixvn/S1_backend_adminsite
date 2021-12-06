import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import { CurrencyInfo } from './CurrencyInfo';
import { api } from '../../../module/WebAPI';

const App = () => {
    return (
        <div className="container-fluid">
            <h4 className="page-name">Deposit Limit Management</h4>
            <div>
                <DepositLimitInfo />
            </div>
        </div>
    )
}

const DepositLimitInfo = () => {
    const [opt, setOpt] = useState({
        siteId: 0, depositType: 0, siteCurrencyList: []
    });
    const [site, setSite] = useState([]);
    const optSite = (siteId) => {
        if (siteId !== opt.siteId && !loading) {
            setOpt({ ...opt, siteId: siteId });
        }
    }
    const [deposit, setdeposit] = useState([]);
    const optDeposit = (depositType) => {
        if (opt.depositType !== depositType && !loading) {
            setOpt({ ...opt, depositType: depositType });
        }
    }
    const [loading, setLoading] = useState();

    let siteRadio = "", depositRadio = "";

    useEffect(() => {
        api.post('/api/Common/AdminUserSiteList')
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    setSite(response.data === null ? [] : response.data);
                } else {
                    // alert(response.message);
                }
            }).catch(function (error) {
                alert(error);
            })

        api.post('/api/Common/GetEnumType?enumName=DepositTypeEnum')
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    setdeposit(response.data === null ? [] : response.data);
                } else {
                    // alert(response.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }, []);

    useEffect(() => {
        let mounted = true;
        if (opt.siteId !== 0 && opt.depositType !== 0 && !loading) {
            setLoading(true);
            setOpt({ ...opt, siteCurrencyList: [] });
            api.post('/api/Deposit/GetDepositLimitCurrencyList', { SiteId: opt.siteId, DepositType: opt.depositType })
                .then(function (response) {
                    if (mounted) {
                        if (response.data.errorCode === 0) {
                            if (response.data.data.length !== 0) {
                                let optCurrencyList = [];
                                response.data.data.map((value) => {
                                    return optCurrencyList.push(value.currencyId);
                                })
                                setOpt({ ...opt, siteCurrencyList: optCurrencyList });
                            } else {
                                // alert(response.message);
                            }
                        }
                        setLoading(false);
                    }
                }).catch(function (error) {
                    if (mounted) {
                        setLoading(false);
                        alert(error);
                    }
                })
        }

        return () => mounted = false;
    }, [opt.siteId, opt.depositType])

    if (site.length === 0 || deposit.length === 0) {
        return <></>
    } else {
        siteRadio = site.data.map((value) => {
            return (
                <div key={`SiteOpt-${value.siteId}`} className="custom-control custom-radio custom-control-inline">
                    <input type="radio" id={`SiteId_${value.siteId}`} name="SiteList" className="custom-control-input" value={value.siteId} checked={(value.siteId === opt.siteId) ? true : false} onChange={optSite.bind(null, value.siteId)} />
                    <label className="custom-control-label" htmlFor={`SiteId_${value.siteId}`}>{value.siteName}</label>
                </div>
            )
        });

        depositRadio = deposit.data.map((value) => {
            return (
                <div key={`DepositOpt-${value.value}`} className="custom-control custom-radio custom-control-inline">
                    <input type="radio" id={`DepositOpt${value.value}`} name="DepositList" className="custom-control-input" value={value.value} checked={(value.value === opt.depositType) ? true : false} onChange={optDeposit.bind(null, value.value)} />
                    <label className="custom-control-label" htmlFor={`DepositOpt${value.value}`}>{value.name}</label>
                </div>
            )
        });
    }

    return (
        <>
            <div className="form-group">
                <label className="col-form-label d-block">Site Name</label>
                {siteRadio}
            </div>
            <div className="form-group">
                <label className="col-form-label d-block">Deposit Type</label>
                {depositRadio}
            </div>
            {opt.siteCurrencyList.length === 0 ? null : loading ? 'Loading...' : <CurrencyInfo optSite={opt} />}
        </>
    )
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}