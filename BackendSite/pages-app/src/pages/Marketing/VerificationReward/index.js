import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import RadioGroup from "./Components/RadioGroup"
import WebAPI from "../../../module//WebAPI";
import TabButtonGroup from "./Components/TabButtonGroup"
import SubPanel from "./SubPanel";

const App = () => {
    const [siteList, setSiteList] = useState([]);
    const [siteOptions, setSiteOptions] = useState([]);
    const [currencyOptions, setCurrencyOptions] = useState([]);
    const [reward, setReward] = useState({});
    const [copyReward, setCopyReward] = useState({});
    const [isEdit, setFlag] = useState(false);
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

    const selectSite = (id) => {

        var site = siteList.find(item => {
            return item.siteId === id;
        });
        let currencyOpts = mapCurrency(site.iCurrencyList)
        setCurrencyOptions(currencyOpts)
        setCurrent({ siteId:id, currencyId: currencyOpts[0].value, currencyName: currencyOpts[0].displayText })

        getVerificationReward(id, currencyOpts[0].value)
    }

    const switchStatus = (status) => {
        let param = {
            Url: "VerificationReward/UpdateStatus",
            Data: { SiteId: current.siteId, CurrencyId: current.currencyId, Status: status^= 1 },
        };
          
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                getVerificationReward(current.siteId, current.currencyId)
                alert("success.");
            } else {
                alert(res.data.message);
            }
        });
    }

    const inputReward = (value, column) => {
        switch (column) {
            case 'phoneReward':
                setReward({ ...reward, phoneReward: value })
                break;
            case 'emailReward':
                setReward({ ...reward, emailReward: value })
                break;
        }
    }

    const toggleType = (value) => {
        setReward({ ...reward, verificationType: value })
    }
    
    const clickEdit = () => {
        setFlag(!isEdit)
    }

    const clickCancel = () => {
        setReward({ ...copyReward })
        setFlag(!isEdit)
    }

    const clickSummit = () => {
        let param = {
            Url: "VerificationReward/Set",
            Data: { 
                SiteId: current.siteId, 
                CurrencyId: current.currencyId, 
                VerificationType: reward.verificationType,
                PhoneReward: reward.phoneReward,
                EmailReward: reward.emailReward
            }
        };
          
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                getVerificationReward(current.siteId, current.currencyId)
                setFlag(!isEdit)
                alert("success.");
            } else {
                alert(res.data.message);
            }
        });
        
    }

    const clickTab = (id, name) => {
        getVerificationReward( current.siteId, id)
        setCurrent({ ...current, currencyId:id, currencyName: name })
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

    const getVerificationReward = (siteId, currencyId) => {
        if(isEdit === true) {
            setFlag(false)
        }
        WebAPI.PostData({ Url: "VerificationReward/Get", Data: { SiteId: siteId, CurrencyId: currencyId } }).then((res) => {
            if (res.data.errorCode === 0) {
                if(res.data.data !== null) {
                    setReward(res.data.data)
                    setCopyReward(res.data.data)
                } else {
                    let initial = {
                        verificationType: 0,
                        phoneReward: 0,
                        emailReward: 0,
                        status: 0
                    }
                    setReward(initial);
                    setCopyReward(initial)
                }
            } else {
              alert(res.data.message ?? "failed to get data.");
            }
        });
    }

    return (
        <div className="container-fluid">
            <h4 className="page-name">Verification Reward</h4>
            <div className="filter noBGcolor"><RadioGroup title="Available Sites" options={siteOptions} toggle={selectSite} selected={current.siteId} /></div>
            <div className="filter noBGcolor tag">
                <TabButtonGroup currencyOptions={currencyOptions} activeTab={current} clickTab={clickTab}/>
            </div>
            <div className="filter">
                {Object.keys(reward).length === 0 ? null : 
                    <SubPanel isEdit={isEdit} 
                              reward={reward} 
                              currencyTitle={current.currencyName} 
                              switchStatus={switchStatus} 
                              clickEdit={clickEdit}
                              clickCancel={clickCancel}
                              clickSubmit={clickSummit}
                              inputReward={inputReward}
                              toggleType={toggleType}
                              />}
            </div>
        </div>
    )
}
export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}