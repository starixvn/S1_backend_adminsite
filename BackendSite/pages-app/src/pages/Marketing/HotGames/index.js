import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import WebAPI from "../../../module//WebAPI";
import Edit from "./Edit";
import RadioGroup from "./Components/RadioGroup"
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
                    setDatas({
                        Datas: [...res.data.data]
                    });
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

    const addHotGame = () => {
        setFlag(true)
    }
    const editHotGame = (id) => {
        console.log(id)
    }

    const deleteHotGame = (id) => {
        console.log(id)
    }

    const back = () => {
        setFlag(false)
    }

    const insertOrUpdate = (hotGame) => {
        let param = {
            Url: "HotGame/Set",
            Data: {
                Id: hotGame.id,
                SiteId: hotGame.siteId,
                CurrencyId: hotGame.currencyId,
                GameName: hotGame.gameName,
                GameId: hotGame.gameId,
                TpId: hotGame.tpId,
                TpName: hotGame.tpName,
                TpGameType: hotGame.tpGameType,
                TpCategory: hotGame.tpCategory,
                GameLogo: {
                    HotGamePcUrl: hotGame.hotGamePcUrl,
                    HotGameMobileUrl: hotGame.hotGameMobileUrl
                },
                TpLogo: hotGame.tpLogo
            }
        };

        WebAPI.PostData(param).then((res) => {

            if (res.data.errorCode === 0) {
                alert("success.");
                setFlag(false)
                getHotGames(current.siteId, current.currencyId)
            } else {
                alert(res.data.message);
            }
        });
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
            <div className="filter">
                <div className="filter noBGcolor"><RadioGroup title="Available Sites" options={siteOptions} toggle={selectSite} selected={current.siteId} disabled={isEdit} /></div>
                {current.currencyId !== 0 ? <div className="filter noBGcolor"><RadioGroup title="Available Currency" options={currencyOptions} toggle={clickTab} selected={current.currencyId} disabled={isEdit} /></div> : null}
            </div>

            {isEdit ?
                <Edit current={current} back={back} insertOrUpdate={insertOrUpdate} /> :
                <>
                    {datas.Datas !== null ?
                        <>
                            <button className="btn btn-add" style={{ marginBottom: "20px" }} onClick={addHotGame}>Create Banner</button>
                            <Result {...datas} ClickEdit={editHotGame} ClickDelete={deleteHotGame} />
                        </> :
                        null
                    }
                </>
            }


        </div>
    )

}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}