import React, { useState, useEffect } from "react";
import InputBox from "./Components/InputBox";
import DropDownBox from "./Components/DropDownBox";
import WebAPI from "../../../module//WebAPI";

function Edit({ id, current, back = f => f, Set = g => g }) {

    const [tpList, setTpList] = useState([]);
    const [gameTypeList, setGameTypeList] = useState([]);

    const [hotGame, setHotGame] = useState({
        id: id,
        siteId: current.siteId,
        currencyId: current.currencyId,
        gameName: "",
        gameId: "",
        tpId: 1,
        tpName: "Saba",
        tpGameType: 1,
        tpCategory: 1,
        tpCategoryName: "Sports",
        hotGamePcUrl: "",
        hotGameMobileUrl: ""
    });

    useEffect(() => {
        WebAPI.fetchData({ Url: "HotGame/GetCommonTpList" }).then((res) => {
            if (res.data.errorCode === 0) {
                setTpList(res.data.data);
            } else {
                alert(res.data.message ?? "failed to get data.");
            }
        });

        WebAPI.fetchData({ Url: "HotGame/GetTpGameType" }).then((res) => {
            if (res.data.errorCode === 0) {
                setGameTypeList(res.data.data);
            } else {
                alert(res.data.message ?? "failed to get data.");
            }
        });

        if (id !== 0) {
            WebAPI.PostData({ Url: "HotGame/GetDetail", Data: `id=${id}` }).then((res) => {
                if (res.data.errorCode === 0) {
                    setHotGame(res.data.data);
                } else {
                    alert(res.data.message ?? "failed to get data.");
                }
            });
        }

    }, [])

    const mapTpOptions = (tpList) => {
        return tpList.map(i => {
            return {
                value: i.id,
                displayText: i.tpName
            };
        });
    };

    const mapGameTypeOptions = (tpList) => {
        return tpList.map(i => {
            return {
                value: i.type,
                displayText: i.typeName
            };
        });
    };

    const selecteTpIdChanged = (value) => {
        let tp = tpList.find(i => {
            return i.id === parseInt(value)
        })
        setHotGame({ ...hotGame, tpId: value, tpName: tp.tpName })
    }

    const selectTpGameTypeChanged = (value) => {
        let category = gameTypeList.find(function (item) {
            return item.type === parseInt(value);
        });
        setHotGame({ ...hotGame, tpGameType: value, tpCategory: category.categoryId, tpCategoryName: category.categoryName })
    }

    const textInput = (value, dataColumn) => {
        setHotGame({ ...hotGame, [dataColumn]: value })
    }

    const submit = () => {
        Set(hotGame)
    }
    return (
        <div className="container-fluid" id="Panel">
            <div className="filter noBGcolor"></div>
            <div className="panel level-1 mt-20">
                <div
                    className="panel-title no-arrow"
                    style={{ justifyContent: "space-between" }}>
                    <span style={{ color: "white" }}>HotGame Info</span>
                </div>
            </div>
            <div className="filter">
                <div className="row-flex">
                    <InputBox title="GameName" value={hotGame.gameName} dataColumn="gameName" onChange={textInput} />
                    <InputBox title="GameId" value={hotGame.gameId} dataColumn="gameId" onChange={textInput} />
                </div>
                <div className="row-flex">
                    <DropDownBox
                        columnTitle="Tp Name"
                        options={mapTpOptions(tpList)}
                        dataColumn="tpId"
                        onChange={selecteTpIdChanged}
                        value={hotGame.tpId}
                    />
                </div>
                <div className="row-flex">
                    <DropDownBox
                        columnTitle="Tp GameType"
                        options={mapGameTypeOptions(gameTypeList)}
                        dataColumn="tpGameType"
                        onChange={selectTpGameTypeChanged}
                        value={hotGame.tpGameType}
                    />
                    <InputBox title="TpCategory" value={hotGame.tpCategoryName} dataColumn="categoryName" disabled={true} />
                </div>
            </div>
            <div className="filter">
                <div className="panel level-1 mt-20">
                    <div className="panel-body">
                        <InputBox title="HotGame Pc Pic Url" value={hotGame.hotGamePcUrl} dataColumn="hotGamePcUrl" className="form-group" inputClass="form-control" onChange={textInput} />
                        <InputBox title="HotGame Mobile Pic Url" value={hotGame.hotGameMobileUrl} dataColumn="hotGameMobileUrl" className="form-group" inputClass="form-control" onChange={textInput} />
                    </div>
                </div>
            </div>

            <div className="filter">
                <button className="btn btn-back" onClick={back}>Back</button>
                <button className="btn btn-add" onClick={submit}>Submit</button>
            </div>
        </div>
    );
}

export default Edit