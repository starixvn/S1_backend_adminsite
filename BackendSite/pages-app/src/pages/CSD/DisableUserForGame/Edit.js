import React, { useState, useEffect, useReducer } from "react";
import TpCheckList from "./Components/TpCheckList";
import TabButton from "./Components/TabButton"
import InputBox from "./Components/InputBox";
import SiteRadioGroup from "./Components/RadioGroup";
import WebAPI from "../../../module//WebAPI";

function reducer (state, action) {
    switch (action.type) {
        case 'initial' :
            return action.data.data;
        case "tpCheck": 
            let idx = state.findIndex(x => x.tpId === action.data.id);
            state[idx].checked = !action.data.checked
            state[idx].gameList = action.data.gameList
            return [...state];
        case "gameCheck":
            const [tpId, game] = action.data.id.split(/_(.+)/)

            state.forEach(a => {
                if(a.tpId === parseInt(tpId)) {
                    a.gameList.forEach(b => {
                        if(b.gameId === game)
                            b.checked = !action.data.checked
                    })
                    
                 }
            });
            return [...state];
        default:
            return [...state];
    }
}

function Edit({ IsAdd, CustId, ClickBack = f => f, InsertOrUpdate = g => g }) {
    
    const [tpList, dispatch] = useReducer(reducer, []);
    const [siteList, setSiteList] = useState([]);
    const [siteId, setSite] = useState(0);
    const [userName, setUserName] = useState("");
    useEffect(() => {        

        WebAPI.fetchData({ Url: "Risk/GetTpList" }).then((res) => {
            if (res.data.errorCode === 0) {
                dispatch({ type: "initial", data: { data: res.data.data}  });
            } 
            else {
                alert(res.data.message ?? "failed to get data.");
            }
        });

        WebAPI.PostData({ Url: "Common/AdminUserSiteList" }).then((res) => {
            if (res.data.errorCode === 0) {
                setSiteList(res.data.data);
            } 
            else {
                alert(res.data.message ?? "failed to get data.");
            }
        });
        
        if(CustId != 0) {
            WebAPI.fetchData({ Url: `Risk/GetTpDisableUserByCustId/${CustId}` }).then((res) => {
                if (res.data.errorCode === 0) {
                    let temp = res.data.data;
                    setUserName(temp.userName)
                    setSite(temp.siteId)
                    dispatch({ type: "initial", data: { data: temp.disableGames}  });
                } 
                else {
                    alert(res.data.message ?? "failed to get data.");
                }
            });
        }
    }, []);

    const Submit = () => {
        if (userName === null || userName === '') {
            alert("Please enter UserName");
            return false;
        }
        let selectedTp = [];
        tpList.forEach( a => {
            if(a.checked) {
                let games = []
                a.gameList.forEach( b => {
                    if(b.checked) {
                        games.push(b.gameId)
                    }
                })
                selectedTp.push({ TpId:a.tpId, GameList:games })
            }
        })

        InsertOrUpdate(userName, siteId, selectedTp)
    };

    const GetTpGameList = (tpId, checked) => {
        
        if(!checked) {    
            WebAPI.fetchData({ Url: `Risk/GameCasinoByTpIdV3/${tpId}` }).then((res) => {
                if (res.data.errorCode === 0) {
                    dispatch({ type: "tpCheck", data: { id: tpId, checked: checked, gameList: res.data.data}  });
                } else {
                  alert(res.data.message ?? "failed to get data.");
                }
              });
        }
        else
        {
            dispatch({ type: "tpCheck", data: { id: tpId, checked: checked, gameList: []} });
        }
    }

    const GameCheck = (gameId, checked) => {
        
        dispatch({ type: "gameCheck", data: { id: gameId, checked: checked} });
    }

    return (
        <>
        <div className="filter noBGcolor"><SiteRadioGroup title="Available Sites" siteList={siteList} selectSite={siteId} disabled={!IsAdd} toggle={setSite}/></div>
        <div className="row-flex"><InputBox title="UserName" value={userName} disabled={!IsAdd} onChange={setUserName} dataColumn = "UserName"/></div>
        <div className="filter noBGcolor tag"><TabButton title="Slots"/></div>
        <div className="filter"><TpCheckList tpList={tpList} 
                                tpChecked={GetTpGameList} 
                                gameChecked={GameCheck}/></div>
        <div className="filter float-btn-group">
            <button className="btn btn-back" onClick={ClickBack}>Back</button>
            <button className="btn btn-add" onClick={Submit}> Submit</button>
        </div>
        </>
    );
}

export default Edit