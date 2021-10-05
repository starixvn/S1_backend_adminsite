import { number_format } from "../../commJS/common";
import { Pagination } from "../../commJS/Pagination.jsx";
import { NewPagination } from "../../commJS/NewPagination.jsx";
import moment from "moment-timezone";
import TpBetRecordDetailModal from "./PopboxForTpBetRecordDetail";

const Result = (props) => {
  let rows =
    props.Datas.data.length === 0 ? (
      <tr>
        <td>No Data.</td>
        <td />
        <td />
        <td />
        <td />
        <td />
        <td />
        <td />
        <td />
      </tr>
    ) : (
      props.Datas.data.map((d, dIdx) => {
        return (
          <tr key={`result-${dIdx}`}>
            {/* <td>
              <button
                className="btn btn-link"
                onClick={() =>
                  props.ClickTpBetRecordDetail(
                    props.Info.GameTypeId,
                    d.transId,
                    d.settledTime
                  )
                }
              >
                {d.transId}
              </button>
            </td> */}
            <td>{d.transId}</td>
            <td>{d.settlementTypeName}</td>
            <td>{number_format(d.turnOver_ExcludeValidTO)}</td>
            <td>{number_format(d.validBet_Valid)}</td>
            <td>{number_format(d.winlostAmount)}</td>
            <td>{moment(d.transTime).format("l LTS")}</td>
            <td>{moment(d.settledTime).format("l LTS")}</td>
            <td>{d.bonusId}</td>
            <td>{d.bonusCode}</td>
          </tr>
        );
      })
    );
  return (
    <>
      <div className="table">
        <label class="col-form-label d-block">Statement By Game</label>
        <table className="table table-striped">
          <thead>
            <tr>
              <th>Trans Id</th>
              <th>SettlementTypeName</th>
              <th>Turnover(ExcludeValidTO)</th>
              <th>Valid Bet</th>
              <th>WinLoss</th>
              <th>Trans Time</th>
              <th>Settled Time</th>
              <th>Bonus Id</th>
              <th>Bonus Code</th>
            </tr>
          </thead>
          <tbody>{rows}</tbody>
        </table>
      </div>
      <NewPagination
        Data={{
          totalPages: props.Datas.totalPages,
          pageNumber: props.Datas.pageNumber,
          rowCount: props.Datas.rowCount,
        }}
        ChangePage={props.ClickDetail.bind(
          null,
          props.Info.Date,
          props.Info.GameTypeId
        )}
      />
      <button
        type="button"
        className="btn btn-back"
        onClick={props.ClickBack.bind(null, "category")}
        style={{ marginTop: "1rem" }}
      >
        Back
      </button>
      <TpBetRecordDetailModal
        show={props.TpBetRecordDatasShowStatus}
        showRawDetail={props.TpBetRecordDatasShowRawDetail}
        GameTypeInfo={props.Info}
        tpBetRecordDatas={props.TpBetRecordDatas}
        handleSwitch={props.SwitchDetailBtn}
        handleClose={(e) => props.TpBetRecordCloseBtn()}
      />
    </>
  );
};

export default Result;
