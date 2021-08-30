import React, { useState, useEffect } from "react";
import TpCheckList from "./Components/TpCheckList";
import TabButton from "./Components/TabButton"
import InputBox from "./Components/InputBox";
import SiteRadioGroup from "./Components/RadioGroup";
import WebAPI from "../../../module//WebAPI";

function Edit({ IsAdd, CustId, ClickBack = f => f, InsertOrUpdate = g => g }) {

    const [tpList, setTpList] = useState([]);
    const [siteList, setSiteList] = useState([]);
    const [siteId, setSite] = useState(0);
    const [userName, setUserName] = useState("");

    useEffect(() => {        

        WebAPI.fetchData({ Url: "Risk/GetTpList" }).then((res) => {
            if (res.data.errorCode === 0) {
                setTpList(res.data.data);
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
                    console.log(res.data.data);
                    setSite(res.data.data.siteId);
                    setUserName(res.data.data.userName);
                } 
                else {
                    alert(res.data.message ?? "failed to get data.");
                }
            });
        }
  
    }, []);
    
    const Submit = () => {
        var selectedTp = [];
        tpList.forEach( (a, i) => {
            if(a.checked) {
                let games = []
                
                a.gameList.forEach( b => {
                    if(b.checked) {
                        games.push(b.gameId)
                    }
                })
                selectedTp.push( { TpId:a.tpId, GameList:games } )
            }
        })

        InsertOrUpdate(userName, siteId, selectedTp)
    };

    const GetTpGameList = (tpId, checked) => {

        console.log(tpId)
        console.log(checked)
        var result = tpList.map((item, index) => {
            return item.tpId;
          }).indexOf(tpId);
        tpList[result].checked = !checked
        
        if(!checked)
        {    
            WebAPI.fetchData({ Url: `Risk/GameCasinoByTpIdV3/${tpId}` }).then((res) => {
                if (res.data.errorCode === 0) {
                    tpList[result].gameList=res.data.data
                    setTpList([...tpList]);
                } else {
                  alert(res.data.message ?? "failed to get data.");
                }
              });
        }
        else
        {
            tpList[result].gameList = []
            setTpList([...tpList]);
        }
    }

    const GameCheck = (gameId, checked) => {
        
        const tpGameId = gameId.split(/_(.+)/)
        var tpIdx = tpList.map( i => {
            return i.tpId;
          }).indexOf(parseInt(tpGameId[0]));

        var gameIdx = tpList[tpIdx].gameList.map( j => {
            return j.gameId;
        }).indexOf(tpGameId[1]);
        
        tpList[tpIdx].gameList[gameIdx].checked = !checked
        setTpList([...tpList]);
    }

    const SelectSite = (siteId) => {
        setSite(siteId);
    }

    const InputUserName = (e) => {
        setUserName(e);
    }

    return (
        <>
        <div className="filter noBGcolor"><SiteRadioGroup title="Available Sites" siteList={siteList} selectSite={siteId} disabled={!IsAdd} toggle={SelectSite}/></div>
        <div className="row-flex"><InputBox title="UserName" value={userName} disabled={!IsAdd} onChange={setUserName}/></div>
        <div className="filter noBGcolor tag"><TabButton title="Slots"/></div>
        <div className="filter"><TpCheckList tpList={tpList} tpChecked={GetTpGameList} gameChecked={GameCheck}/></div>
        <div className="filter float-btn-group">
            <button className="btn btn-back" onClick={ClickBack}>Back</button>
            <button className="btn btn-add" onClick={Submit}> Submit</button>
        </div>
        </>
    );
}

export default Edit