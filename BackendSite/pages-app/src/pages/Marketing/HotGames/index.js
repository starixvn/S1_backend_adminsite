import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import WebAPI from "../../../module//WebAPI";
import RadioGroup from "./Components/RadioGroup"
import TabButtonGroup from "./Components/TabButtonGroup"
import Result from "./Result";


const App = () => {
    const [siteList, setSiteList] = useState([]);
    const [siteOptions, setSiteOptions] = useState([]);
    const [currencyOptions, setCurrencyOptions] = useState([]);
    const [isEdit, setFlag] = useState(false);
    const [datas, setDatas] = useState({ Datas: null });
    const [current, setCurrent] = useState({ siteId: 0, currencyId: 0, currencyName: "" });

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
        setCurrent({ siteId: id, currencyId: currencyOpts[0].value, currencyName: currencyOpts[0].displayText })

        getHotGames(id, currencyOpts[0].value)
    }

    const getHotGames = (siteId, currencyId) => {
        if (isEdit === true) {
            setFlag(false)
        }
        WebAPI.PostData({ Url: "HotGame/GetList", Data: { SiteId: siteId, CurrencyId: currencyId } }).then((res) => {
            if (res.data.errorCode === 0) {
                if (res.data.data !== null) {
                    console.log(res.data.data)
                    setDatas({
                        Datas: [...res.data.data]
                    });
                } else {
                    let initial = {
                        verificationType: 0,
                        phoneReward: 0,
                        emailReward: 0,
                        status: 0
                    }
                }
            } else {
                alert(res.data.message ?? "failed to get data.");
            }
        });
    }

    const clickTab = (id, name) => {
        getHotGames(current.siteId, id)
        setCurrent({ ...current, currencyId: id, currencyName: name })
    }

    const editHotGame = (id) => {
        console.log(id)
    }

    const deleteHotGame = (id) => {
        console.log(id)
    }

    const mapSite = siteList => {
        return siteList.map((item) => {
            return { value: item.siteId, displayText: item.siteName }
        });
    }

    const mapCurrency = currencyList => {
        return currencyList.map(i => {
            return {
                value: Number(i),
                displayText: window.i18n[`lbl_Currency${i}`]
            };
        });
    };

    return (
        <div className="container-fluid">
            <h4 className="page-name">HotGames</h4>
            <div className="filter noBGcolor"><RadioGroup title="Available Sites" options={siteOptions} toggle={selectSite} selected={current.siteId} /></div>
            <div className="filter noBGcolor tag">
                <TabButtonGroup currencyOptions={currencyOptions} activeTab={current} clickTab={clickTab} />
            </div>
            {datas.Datas === null ? null : (
                <Result {...datas} ClickEdit={editHotGame} ClickDelete={deleteHotGame} />
            )}
        </div>
    )

}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}