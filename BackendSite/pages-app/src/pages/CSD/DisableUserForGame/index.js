import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import data from "./data.json";
import qs from "qs";
import WebAPI from "../../../module//WebAPI";

const App = () => {
  const [stateDatas, updateDatas] = useState({
    Datas: null,
    ShowSettingPanel: false,
    IsAdd: false,
  });

  const InsertDisableUserForGame = (datas, disableGames) => {
    let insertData = {
      CustId: datas.custId,
      SiteId: datas.siteId,
      TpGame: disableGames
    };
    console.log("Hello")
    let param = {
      Url: "Risk/TpDisableUserForGame",
      Data: qs.stringify(insertData),
    };
    
    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        alert("success.");
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };
  return (
    <div className="container-fluid">
      <h4 className="page-name">Disable User For Game</h4>
      <button 
        className="btn btn-add"
        style={{ marginBottom: "20px" }}
        onClick={()=>{

          let custInfo = {
            custId : 250140,
            siteId : 1
          }

          InsertDisableUserForGame(custInfo, data)
          }}>
        Disable User For Game
      </button>
    </div>
  );
}


export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}