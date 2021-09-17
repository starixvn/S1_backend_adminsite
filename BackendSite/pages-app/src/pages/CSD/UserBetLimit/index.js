import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import Edit from "./Edit"
import qs from "qs";
import WebAPI from "../../../module//WebAPI";
const App = () => {
  const [eventState, updateEvent] = useState({
    ShowSettingPanel: false,
    IsAdd: true,
    CustId: 0
  });


  const InsertOrUpdate = (userName, siteId, betSetting) => {

    let data = {
      UserName: userName,
      SiteId: siteId,
      BetSetting: betSetting
    };
    let param = {
      Url: "UserBetLimit/InsertOrUpdate",
      Data: qs.stringify(data),
    };
    
    WebAPI.PostData(param).then((res) => {

      if (res.data.errorCode === 0) {
        alert("success.");
        updateEvent({
          ...eventState,
          ShowSettingPanel: false,
        });
      } else {
        alert(res.data.message);
      }
    });
  };

  return (
    <div className="container-fluid">
      {eventState.ShowSettingPanel ? 
        <Edit 
          ClickBack={() => {
            updateEvent({
              ...eventState,
              ShowSettingPanel: false,
            });
          }}
          IsAdd={eventState.IsAdd}
          CustId = {eventState.CustId}
          InsertOrUpdate={InsertOrUpdate} /> : 
        <>
        <h4 className="page-name">User Bet Limit</h4>
          <button
            className="btn btn-add"
            style={{ marginBottom: "20px" }}
            onClick={() => {
                updateEvent({
                  ...eventState,
                  ShowSettingPanel: true,
                  IsAdd: true,
                  CustId: 0
                });
            }}>
            Create
          </button>
        </>
      }
    </div>
  );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}