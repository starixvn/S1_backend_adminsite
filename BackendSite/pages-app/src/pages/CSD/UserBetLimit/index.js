import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import Edit from "./Edit"
import Result from "./Result";
import qs from "qs";
import WebAPI from "../../../module//WebAPI";
import moment from "moment";

const App = () => {
  const [eventState, updateEvent] = useState({
    ShowSettingPanel: false,
    IsAdd: true,
    CustId: 0
  });

  const [datas, setDatas] = useState({
    PageInfo: null,
    Datas: null
  });

  useEffect(() => {
    GetUserBetLimitData();
  }, []);

  const GetUserBetLimitData = (pageNumber = 1, pageSize = 10) => {

    let query = {
      SiteIds: [1,2,3],
      UserName: null,
      StartDate: moment('2021-09-15').utc().unix(),
      EndDate: moment('2021-09-22').utc().unix(),
      pageNumber: pageNumber,
      PageSize: pageSize
    };

    let param = {
      Url: "UserBetLimit/GetUserBetLimit",
      Data: qs.stringify(query),
    };

    WebAPI.PostData(param).then((res) => {

      if (res.data.errorCode === 0) {
        setDatas({
          PageInfo: {
            totalPages: res.data.totalPages,
            pageNumber: res.data.pageNumber,
            rowCount: res.data.rowCount
          },
          Datas: [...res.data.data]
        });
      } else {
        alert(res.data.message);
      }
    });
  }

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

  const Result_ChangePage = () => {

  }

  const EditDisableUser = () => {

  }

  const DeleteDisableUser = () => {

  }

  const SwitchStatus = () => {

  }

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
          {datas.Datas === null ? null : (
              <Result
              {...datas}
              ChangePage={Result_ChangePage}
              ClickEdit={EditDisableUser}
              ClickDelete={DeleteDisableUser}
              UpdateStatus={SwitchStatus} />
          )}
        </>
      }
    </div>
  );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}