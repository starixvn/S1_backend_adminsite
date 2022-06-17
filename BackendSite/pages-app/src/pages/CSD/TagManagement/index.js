import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment-timezone";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
import Edit from "./Edit";

const App = () => {
  moment.tz.setDefault("Asia/Taipei");
  const [state, setstate] = useState(0);
  const [showAll, setshowAll] = useState([]);
  const [editData, seteditData] = useState({
    Id: 0,
    TagName: "",
    TagColour: "#fff",
    Description: "",
  });
  const [origData, setOrigData] = useState({});
  const [stateDatas, setDatas] = useState({
    PageInfo: { pageNumber: 1 },
    Datas: null,
    FilterInfo: { status: 0 },
  });

  const Create = () => {
    setstate(2);
  };

  const EditOnChange = (v, dataColumn) => {
    seteditData({
      ...editData,
      [dataColumn]: v,
    });
  };

  const ChangeColor = (color) => {
    seteditData({
      ...editData,
      TagColour: color.hex,
    });
  };

  const ShowAllCheckBox = (value, dataColumn) => {
    if (showAll.length > 0) {
      setshowAll([]);
    } else {
      setshowAll([...showAll, value]);
    }
  };

  const EditSubmit = () => {

    let Data = {
      currencyId: 0,
      siteId: 0,
      before: {
        id: origData.id,
        description: origData.description,
        tagName: origData.tagName,
        tagColour: origData.tagColour
      },
      after: {
        id: editData.Id,
        description: editData.Description,
        tagName: editData.TagName,
        tagColour: editData.TagColour
      }
    };
    api
      .post("/apiv2/TagManagement/EditTagManagement", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          //EditBack();
          seteditData({
            Id: 0,
            TagName: "",
            TagColour: "#fff",
            Description: "",
          });
          Resubmit();
          setstate(0);
          setOrigData({});
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const EditBack = () => {
    seteditData({
      Id: 0,
      TagName: "",
      TagColour: "#fff",
      Description: "",
    });
    //Resubmit();
    setstate(0);
    setOrigData({});
  };

  const ClickSubmit = () => {
    let tempstatus = 0;
    if (showAll.length > 0) {
      tempstatus = 1;
    }
    let Data = {
      Status: tempstatus,
      PageSize: parseInt(10),
      PageNumber: parseInt(1),
    };
    api
      .post("/api/CustManage/GetTagData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            PageInfo: {
              ...stateDatas.PageInfo,
              pageNumber: 1,
              totalPages: res.data.totalPages,
              rowCount: res.data.rowCount,
            },
            Datas: [...res.data.data],
            FilterInfo: {
              status: tempstatus,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const ChangePage = (PageNumber) => {
    let tempstatus = 0;
    if (showAll.length > 0) {
      tempstatus = 1;
    }
    let Data = {
      Status: tempstatus,
      PageSize: parseInt(10),
      PageNumber: parseInt(PageNumber),
    };
    api
      .post("/api/CustManage/GetTagData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            PageInfo: {
              ...stateDatas.PageInfo,
              pageNumber: PageNumber,
              totalPages: res.data.totalPages,
              rowCount: res.data.rowCount,
            },
            Datas: [...res.data.data],
            FilterInfo: {
              status: tempstatus,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const Resubmit = () => {
    let Data = {
      Status: stateDatas.FilterInfo.status,
      PageSize: parseInt(10),
      PageNumber: stateDatas.PageInfo.pageNumber,
    };
    api
      .post("/api/CustManage/GetTagData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            Datas: [...res.data.data],
            PageInfo: {
              ...stateDatas.PageInfo,
              totalPages: res.data.totalPages,
              rowCount: res.data.rowCount,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const Detail = (Data) => {
    seteditData({
      ...editData,
      Id: Data.id,
      TagName: Data.tagName,
      TagColour: Data.tagColour,
      Description: Data.description,
    });
    setstate(1);
    setOrigData({
      id: Data.id,
      tagName: Data.tagName,
      tagColour: Data.tagColour,
      description: Data.description,
    });
  };

  const StatusChange = (value) => {
    if (window.confirm(`Are you sure change ${value.tagName} ?`)) {
      let afterStatus = value.status * 1 === 0 ? 1 : 0;
      let Data = {
        currencyId: 0,
        siteId: 0,
        before: {
          id: value.id,
          tagName: value.tagName,
          status: value.status
        },
        after: {
          id: value.id,
          tagName: value.tagName,
          status: afterStatus
        }
      };
      api
        .post("/apiv2/TagManagement/SetTagStatus", Data)
        .then(function (res) {
          if (res.data.errorCode === 0) {
            Resubmit();
          } else {
            alert(res.data.message ?? "failed to get data.");
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };
  if (state === 0) {
    return (
      <div className="container-fluid">
        <h4 className="page-name">Tag Management</h4>
        <Filter
          ShowAllCheckBox={ShowAllCheckBox}
          ShowAll={showAll}
          ClickSubmit={ClickSubmit}
        />
        {stateDatas.Datas === null ? null : (
          <Result
            {...stateDatas}
            Resubmit={Resubmit}
            Detail={Detail}
            StatusChange={StatusChange}
            ChangePage={ChangePage}
          />
        )}
        <div className="row-flex">
          <div className="button-group">
            <button
              className="btn btn-search"
              type="submit"
              onClick={Create}
              style={{
                marginTop: "20px",
                marginBottom: "40px",
                marginRight: "10px",
              }}
            >
              Create
            </button>
          </div>
        </div>
      </div>
    );
  } else if (state === 2 || state === 1) {
    return (
      <div>
        <Edit
          EditData={editData}
          onChange={EditOnChange}
          ChangeColor={ChangeColor}
          Submit={EditSubmit}
          Back={EditBack}
        />
      </div>
    );
  }
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
