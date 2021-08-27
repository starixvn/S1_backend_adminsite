import React, { useState, useEffect } from "react";
import TpCheckList from "./Components/TpCheckList";
import TabButton from "./Components/TabButton"
import WebAPI from "../../../module//WebAPI";

function Edit({ ClickBack = f => f, InsertOrUpdate = g => g }) {

    const [tpList, setTpList] = useState([]);

    useEffect(() => {
  
      WebAPI.PostData({ Url: "Risk/GetTpList" }).then((res) => {
        if (res.data.errorCode === 0) {
            setTpList(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      });
  
    }, []);
    
    const Submit = () => {
        var temp = [];
        tpList.forEach( (a, i) => {
            if(a.checked) {
                let games = []
                
                a.gameList.forEach( b => {
                    if(b.checked) {
                        games.push(b.gameId)
                    }
                })
                temp.push( { TpId:a.tpId, GameList:games } )
            }
        })

        InsertOrUpdate(250140, 1, temp)
    };

    const GetTpGameList = (tpId, checked) => {

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
        const tpGameId = gameId.split('-')
        var tpIdx = tpList.map( i => {
            return i.tpId;
          }).indexOf(parseInt(tpGameId[0]));

        var gameIdx = tpList[tpIdx].gameList.map( j => {
            return j.gameId;
        }).indexOf(tpGameId[1]);
        
        tpList[tpIdx].gameList[gameIdx].checked = !checked
        setTpList([...tpList]);
    }

    return (
        <>
        <div className="filter noBGcolor tag"><TabButton title="Slots"/></div>
        <div className="filter"><TpCheckList tpList={tpList} tpChecked={GetTpGameList} gameChecked={GameCheck}/></div>
        <div className="filter float-btn-group">
            <button
                className="btn btn-back"
                onClick={ClickBack}>
                Back
            </button>
            <button
                className="btn btn-add"
                onClick={Submit}>
                Submit
            </button>
        </div>
        </>
    );
}

export default Edit