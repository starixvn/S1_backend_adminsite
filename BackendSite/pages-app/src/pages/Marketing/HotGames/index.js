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
    const [isEditDetail, setEditFlag] = useState(false);
    const [isEditSort, setSortFlag] = useState(false);
    const [datas, setDatas] = useState({ Datas: null });
    const [current, setCurrent] = useState({ siteId: 0, currencyId: 0, currencyName: "" });
    const [id, setId] = useState(0);

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
        setSortFlag(false)
        getHotGames(id, currencyOpts[0].value)
    }

    const getHotGames = (siteId, currencyId) => {
        if (isEditDetail === true) {
            setEditFlag(false)
        }
        WebAPI.PostData({ Url: "HotGame/GetList", Data: { SiteId: siteId, CurrencyId: currencyId } }).then((res) => {
            if (res.data.errorCode === 0) {
                if (res.data.data !== null) {
                    let hotGameData = res.data.data;
                    setDatas({
                        Datas: [...hotGameData]
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
        setEditFlag(true)
    }
    const editHotGame = (id) => {
        setId(id)
        setEditFlag(true)
    }

    const clickSort = () => {
        setSortFlag(true)
    }

    const sortingCancel = () => {
        setSortFlag(false)
        getHotGames(current.siteId, current.currencyId)
    }

    const sortingSave = () => {
        let sorting = datas.Datas.map(i => {
            return {
                Id: i.id,
                Sort: i.sort
            }
        })

        WebAPI.PostData({ Url: "HotGame/Sorting", Data: sorting }).then((res) => {
            if (res.data.errorCode === 0) {
                alert("success.");
                getHotGames(current.siteId, current.currencyId)
            } else {
                alert(res.data.message);
            }
        });
        setSortFlag(false)
    }

    const sortListChanged = (id, targetSort) => {
        let originSort = datas.Datas.find(a => {
            return a.id === parseInt(id)
        }).sort;

        if (originSort > targetSort) {
            datas.Datas.forEach(h => {
                if (h.sort >= parseInt(targetSort) && h.sort < parseInt(originSort)) {
                    h.sort = parseInt(h.sort) + 1
                }
                if (h.id === id) {
                    h.sort = targetSort
                }
            });
        } else {
            datas.Datas.forEach(h => {
                if (h.sort > parseInt(originSort) && h.sort <= parseInt(targetSort)) {
                    h.sort = parseInt(h.sort) - 1
                }
                if (h.id === id) {
                    h.sort = targetSort
                }
            });
        }
        datas.Datas.sort(function (a, b) {
            return a.sort - b.sort;
        });

        setDatas({ ...datas, Datas: datas.Datas })
    }

    const deleteHotGame = (id) => {

        let originSort = datas.Datas.find(a => {
            return a.id === parseInt(id)
        }).sort;
        datas.Datas.forEach(h => {
            if (h.sort > parseInt(originSort)) {
                h.sort = parseInt(h.sort) - 1
            }
        });
        let index = datas.Datas.findIndex(b => {
            return b.id === id
        });
        if (index > -1) {
            datas.Datas.splice(index, 1);
        }
        setDatas({ ...datas, Datas: datas.Datas })

        WebAPI.PostData({ Url: "HotGame/Delete", Data: `id=${id}` }).then((res) => {
            if (res.data.errorCode === 0) {
                sortingSave();
            } else {
                alert(res.data.message);
            }
        });
    }

    const back = () => {
        setEditFlag(false)
    }

    const Set = (hotGame) => {
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
                setEditFlag(false)
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
                <div className="filter noBGcolor"><RadioGroup title="Available Sites" options={siteOptions} toggle={selectSite} selected={current.siteId} disabled={isEditDetail} /></div>
                {current.currencyId !== 0 ? <div className="filter noBGcolor"><RadioGroup title="Available Currency" options={currencyOptions} toggle={clickTab} selected={current.currencyId} disabled={isEditDetail} /></div> : null}
            </div>

            {isEditDetail ?
                <Edit id={id} current={current} back={back} Set={Set} /> :
                <>
                    {datas.Datas !== null ?
                        <>
                            <button className="btn btn-add" style={{ marginBottom: "20px" }} onClick={addHotGame}>Create HotGame</button>
                            {isEditSort ?
                                <>
                                    <button className="btn btn-add mx-3" style={{ marginBottom: "20px" }} onClick={sortingSave}>Save</button>
                                    <button className="btn btn-add mx-3" style={{ marginBottom: "20px" }} onClick={sortingCancel}>Cancel</button>
                                </> :
                                <button className="btn btn-add mx-3" style={{ marginBottom: "20px" }} onClick={clickSort}>EditSort</button>
                            }
                            <Result {...datas} isEditSort={isEditSort} sortList={[...Array(datas.Datas.length).keys()]} sortListChanged={sortListChanged} ClickEdit={editHotGame} ClickDelete={deleteHotGame} />
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