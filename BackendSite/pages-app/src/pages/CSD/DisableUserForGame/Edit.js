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

        if(!checked)
        {    
            WebAPI.fetchData({ Url: `Risk/GameCasinoByTpIdV3/${tpId}` }).then((res) => {
                if (res.data.errorCode === 0) {
                    tpList.forEach( a => {
                        if(a.tpId === tpId) {
                            a.checked = !checked
                            a.gameList=res.data.data
                        }
                    })
                    setTpList([...tpList]);
                } else {
                  alert(res.data.message ?? "failed to get data.");
                }
              });
        }
        else
        {
            tpList.forEach( a => {
                if(a.tpId === tpId) {
                    a.checked = !checked
                    a.gameList = []
                }   
            })
            setTpList([...tpList]);
        }
    }

    const GameCheck = (gameId, checked) => {
        
        const [tpId, game] = gameId.split(/_(.+)/)

        tpList.forEach( a => {
            if(a.tpId === parseInt(tpId)) {
                a.gameList.forEach( b => {
                    if(b.gameId === game)
                        b.checked = !checked
                })
             }
                
        })

        setTpList([...tpList]);
    }

    return (
        <>
        <div className="filter noBGcolor"><SiteRadioGroup title="Available Sites" siteList={siteList} selectSite={siteId} disabled={!IsAdd} toggle={setSite}/></div>
        <div className="row-flex"><InputBox title="UserName" value={userName} disabled={!IsAdd} onChange={setUserName} dataColumn = "UserName"/></div>
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