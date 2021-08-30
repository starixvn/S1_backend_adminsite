import React from "react";
import Pagination from "./Pagination";
import moment from "moment-timezone";

const SpecialItemType = {
	actionBtn: "actionBtn"
};

const dataColumnDefs = () => {
  return [
    { dataColumn: "siteName", columnTitle: "SiteName" },
    {
      dataColumn: "userName",
      columnTitle: "UserName",
      body: {
        format: userNameFormat,
        subData: ["custId"],
      },
    },
    { dataColumn: "creator", columnTitle: "Creator"},
    {
      dataColumn: "createTime",
      columnTitle: "CreateTime (GMT+8)",
      body: {
        format: dateTimeFormat,
      },
    },
    {
      dataColumn: "updateTime",
      columnTitle: "UpdateTime (GMT+8)",
      body: {
        format: dateTimeFormat,
      },
    },
    { dataColumn: "updateOperator", columnTitle: "LastUpdateUser"},
    {
        columnTitle: "Action",
        header: {
            attr: {
                className: "width-85"
            }
        },
        body: {
            SpecialItem: SpecialItemType.actionBtn
        }
    }
  ];
};

export default function Result (tpDisableUser) {
    const columnDefs = dataColumnDefs();
    let headerRows = [];

    columnDefs.forEach((element, idx) => {
        headerRows.push(
        <th
            key={`th-${element.dataColumn}-${idx}`}
            scope="col"
            className={
            element.header && element.header.attr && element.header.attr.class
            ? element.header.attr.class
            : ""}>
            {element.columnTitle}
        </th>);
    });
    let bodyRows = tpDisableUser.Datas.map((data, i) => {
        let row = columnDefs.map((element, j) => {
            
            let body_content = null;
            if (element.body && element.body.format) {
                let subData = {};
                if(element.body.subData) {
                    element.body.subData.forEach((s) => {
                    subData[s] = data[s];
                    });
                }
                body_content = element.body.format(data[element.dataColumn], subData)

            }
            else if (element.body && element.body.SpecialItem && element.body.SpecialItem === SpecialItemType.actionBtn) {
                body_content = (
                    <ActionButtons ClickEdit={tpDisableUser.ClickEdit.bind(null, data["custId"])}/>
                );
            }
            else {
                body_content = data[element.dataColumn]
            }

            return (
                <td
                key={`td-${element.dataColumn}-${j}`}
                className={
                    element.body && element.body.attr && element.body.attr.class
                    ? element.body.attr.class
                    : ""
                }>
                {body_content}
                </td>
            );
        });

        return <tr key={`result-body-${i}`}>{row}</tr>;
    });

  return (
    <div className="table">
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
      <Pagination
        Data={{
          totalPages: tpDisableUser.PageInfo.totalPages,
          pageNumber: tpDisableUser.PageInfo.pageNumber,
          rowCount: tpDisableUser.PageInfo.rowCount,
        }}
        ChangePage={tpDisableUser.ChangePage}
      />
    </div>
  );
};

const ActionButtons = props => {
	return (
		<div className="button-action">
			<button className="btn btn-edit" title="Only can edit time column." onClick={props.ClickEdit}>
				<i className="icon-edit"></i>
			</button>
		</div>
	);
};

const userNameFormat = (userName, subData) => {
  return (
    <a
      href={
        "/CustManage/CustInfo?UserName=" +
        userName +
        "&custId=" +
        subData.custId +
        "&TransId="
      }
      target="_blank"
    >
      {userName}
    </a>
  );
};

const dateTimeFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};