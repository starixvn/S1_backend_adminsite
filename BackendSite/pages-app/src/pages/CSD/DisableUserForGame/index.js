import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import Edit from "./Edit"
import qs from "qs";
import WebAPI from "../../../module//WebAPI";

const App = () => {

  const [stateDatas, updateDatas] = useState({
    Datas: null,
    ShowSettingPanel: false,
    IsAdd: false,
  });
  
  const UpdateDisableUserForGame = (userName, siteId, disableTpGame) => {

    let data = {
      UserName: userName,
      SiteId: siteId,
      DisableTpGame: disableTpGame
    };
    let param = {
      Url: "Risk/TpDisableUserForGame",
      Data: qs.stringify(data),
    };
    
    WebAPI.PostData(param).then((res) => {

      if (res.data.errorCode === 0) {
        alert("success.");
        updateDatas({
          ...stateDatas,
          ShowSettingPanel: false,
        });
      } else {
        alert(res.data.message);
      }
    });
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">Disable User For Game</h4>
      {stateDatas.ShowSettingPanel ? 
        <Edit 
          ClickBack={() => {
            updateDatas({
              ...stateDatas,
              ShowSettingPanel: false,
            });
          }}
          InsertOrUpdate={UpdateDisableUserForGame}/> : 
      <>
        <button
          className="btn btn-add"
          style={{ marginBottom: "20px" }}
          onClick={() => {
              updateDatas({
                ...stateDatas,
                ShowSettingPanel: true,
                IsAdd: true,
              });
          }}>
          Add Maintenance
        </button>
      </>}
    </div>
  );
}


export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}