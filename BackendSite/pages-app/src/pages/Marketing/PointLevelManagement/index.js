import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import { CurrencyInfo } from './CurrencyInfo';
import {api} from '../../../module/WebAPI';

const App = () => {
    return (
        <div className="container-fluid">
            <h4 className="page-name">Point Level Management</h4>
            <div>
                <PointInfoList />
            </div>
        </div>
    )
}

const PointInfoList = () => {
    const [opt, setOpt] = useState({
        siteId: 0, siteCurrencyList: []
    });
    const [site, setSite] = useState([]);
    const optSite = (siteId, siteCurrencyList) => { setOpt({ siteId, siteCurrencyList }) }
    let radio = "";

    useEffect(() => {
        api.get('/api/Marketing/GetAllSites')
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    setSite(response.data === null ? [] : response.data);
                } else {
                    // alert(response.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }, []);

    if (site.length === 0) {
        return <></>
    } else {
        radio = site.data.map((value) => {
            return (
                <div key={`SiteOpt-${value.siteId}`} className="custom-control custom-radio custom-control-inline">
                    <input type="radio" id={`SiteId_${value.siteId}`} name="SiteList" className="custom-control-input" value={value.siteId} onChange={optSite.bind(null, value.siteId, value.iCurrencyList)} />
                    <label className="custom-control-label" htmlFor={`SiteId_${value.siteId}`}>{value.siteName}</label>
                </div>
            )
        });
    }

    return (
        <>
            <div className="form-group">
                <label className="col-form-label d-block">Site Name</label>
                {radio}
            </div>
            {opt.siteCurrencyList.length === 0 ? null : <CurrencyInfo optSite={opt} />}
        </>
    )
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}