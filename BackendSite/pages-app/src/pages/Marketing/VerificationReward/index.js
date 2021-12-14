import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import RadioGroup from "./Components/RadioGroup"
import WebAPI from "../../../module//WebAPI";
import TabButtonGroup from "./Components/TabButtonGroup"
import SubPanel from "./SubPanel";
import { initVerificationType } from "./defCols";

const App = () => {
    const [siteList, setSiteList] = useState([]);
    const [siteOptions, setSiteOptions] = useState([]);
    const [currencyOptions, setCurrencyOptions] = useState([]);
    const [reward, setReward] = useState({});
    const [siteReward, setSiteReward] = useState([]);
    const [current, setCurrent] = useState({ siteId:0, currencyId:0, currencyName:"" });
    
    useEffect(() => {
        WebAPI.PostData({ Url: "Common/AdminUserSiteList" }).then((res) => {
            if (res.data.errorCode === 0) {

                let siteOpts = mapSite(res.data.data)
                
                setSiteList(res.data.data);
                setSiteOptions(siteOpts);
            } 
            else {
              alert(res.data.message ?? "failed to get data.");
            }
        });
      }, []);

    const SelectSite = (id) => {

        var site = siteList.find(item => {
            return item.siteId === id;
        });
        let currencyOpts = mapCurrency(site.iCurrencyList)
        setCurrencyOptions(currencyOpts)
        setCurrent({ siteId:id, currencyId: currencyOpts[0].value, currencyName: currencyOpts[0].displayText })

        WebAPI.JsonPost({ Url: "VerificationReward/Get", Data: { SiteId: id } }).then((res) => {
            if (res.data.errorCode === 0) {
                setSiteReward(res.data.data)
                let firstSetting = res.data.data[0];
                setReward({ 
                    verificationType: firstSetting.verificationType,
                    phoneReward: firstSetting.phoneReward,
                    emailReward: firstSetting.emailReward,
                    lastUpdateTime: firstSetting.lastUpdateTime,
                    lastUpdateUser: firstSetting.lastUpdateUser,
                    status: firstSetting.status
                })
                
            } else {
              alert(res.data.message ?? "failed to get data.");
            }
        });
    }

    const clickTab = (id, name) => {
        let curReward = siteReward.find(item => {
            return item.currencyId === id
        })
        setReward({
            ...reward,
            verificationType: curReward.verificationType,
            phoneReward: curReward.phoneReward,
            emailReward: curReward.emailReward,
            lastUpdateTime: curReward.lastUpdateTime,
            lastUpdateUser: curReward.lastUpdateUser,
            status: curReward.status
        })
        setCurrent({...current, currencyId:id, currencyName: name })
    }

    const mapCurrency = currencyList => {
        return currencyList.map(i => {
            return {
                value: Number(i),
                displayText: window.i18n[`lbl_Currency${i}`]
            };
        });
    };

    const mapSite = siteList => {
        return siteList.map((item) => {
            return { value: item.siteId, displayText: item.siteName }
        });
    }

    return (
        <div className="container-fluid">
            <h4 className="page-name">Verification Reward</h4>
            <div className="filter noBGcolor"><RadioGroup title="Available Sites" options={siteOptions} toggle={SelectSite} selected={current.siteId} /></div>
            <div className="filter noBGcolor tag">
                <TabButtonGroup currencyOptions={currencyOptions} activeTab={current} clickTab={clickTab}/>
            </div>
            <div className="filter">
                <div className="panel level-1 mt-20">
                    <a className="panel-title no-arrow" style={{ justifyContent: "space-between" }}>
                        <span style={{ color: "white" }}>{current.currencyName}</span>
                    </a>
                </div>  
            {Object.keys(reward).length === 0 ? null : <SubPanel reward={reward} currencyOptions={currencyOptions} current={current} clickTab={clickTab} />} 
            </div>
        </div>
    )
}
export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}